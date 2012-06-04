﻿using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace VSSparkExtension
{
    internal class SparkCompletionCommandHandler : IOleCommandTarget
    {
        private readonly IOleCommandTarget _nextCommandHandler;
        private readonly ITextView _textView;
        private readonly SparkCompletionHandlerProvider _provider;
        private ICompletionSession _session;

        internal SparkCompletionCommandHandler(IVsTextView textViewAdapter, ITextView textView, SparkCompletionHandlerProvider provider)
        {
            _textView = textView;
            _provider = provider;

            //add the command to the command chain
            textViewAdapter.AddCommandFilter(this, out _nextCommandHandler);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return _nextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (VsShellUtilities.IsInAutomationFunction(_provider.ServiceProvider))
            {
                return _nextCommandHandler.Exec(ref pguidCmdGroup, nCmdId, nCmdexecopt, pvaIn, pvaOut);
            }
            //make a copy of this so we can look at it after forwarding some commands
            var commandId = nCmdId;
            var typedChar = char.MinValue;
            //make sure the input is a char before getting it
            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdId == (uint)VSConstants.VSStd2KCmdID.TYPECHAR)
            {
                typedChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
            }

            //check for a commit character
            if (nCmdId == (uint)VSConstants.VSStd2KCmdID.RETURN
                || nCmdId == (uint)VSConstants.VSStd2KCmdID.TAB
                || (char.IsWhiteSpace(typedChar) || char.IsPunctuation(typedChar)))
            {
                //check for a a selection
                if (_session != null && !_session.IsDismissed)
                {
                    //if the selection is fully selected, commit the current session
                    if (_session.SelectedCompletionSet.SelectionStatus.IsSelected)
                    {
                        _session.Commit();
                        //also, don't add the character to the buffer
                        return VSConstants.S_OK;
                    }
                    //if there is no selection, dismiss the session
                    _session.Dismiss();
                }
            }

            //pass along the command so the char is added to the buffer
            var retVal = _nextCommandHandler.Exec(ref pguidCmdGroup, nCmdId, nCmdexecopt, pvaIn, pvaOut);
            var handled = false;
            if (!typedChar.Equals(char.MinValue) && char.IsLetterOrDigit(typedChar))
            {
                if (_session == null || _session.IsDismissed) // If there is no active session, bring up completion
                {
                    TriggerCompletion();
                    if (_session != null) _session.Filter();
                }
                else    //the completion session is already active, so just filter
                {
                    _session.Filter();
                }
                handled = true;
            }
            else if (commandId == (uint)VSConstants.VSStd2KCmdID.BACKSPACE   //redo the filter if there is a deletion
                || commandId == (uint)VSConstants.VSStd2KCmdID.DELETE)
            {
                if (_session != null && !_session.IsDismissed)
                    _session.Filter();
                handled = true;
            }
            return handled ? VSConstants.S_OK : retVal;
        }

        private void TriggerCompletion()
        {
            //the caret must be in a non-projection location 
            var caretPoint =
            _textView.Caret.Position.Point.GetPoint(
            textBuffer => (!textBuffer.ContentType.IsOfType("projection")), PositionAffinity.Predecessor);
            if (!caretPoint.HasValue)
            {
                return;
            }

            _session = _provider.CompletionBroker.CreateCompletionSession
         (_textView,
                caretPoint.Value.Snapshot.CreateTrackingPoint(caretPoint.Value.Position, PointTrackingMode.Positive),
                true);

            //subscribe to the Dismissed event on the session 
            _session.Dismissed += OnSessionDismissed;
            _session.Start();
        }

        private void OnSessionDismissed(object sender, EventArgs e)
        {
            _session.Dismissed -= OnSessionDismissed;
            _session = null;
        }
    }
}