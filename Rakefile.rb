require 'rubygems'
require 'bundler'
require 'albacore'

Bundler.setup

task :default => [:compile]

desc "Compile Solution"
msbuild :compile do |msb|
  msb.properties :configuration => :Debug
  msb.targets :Clean, :Build
  msb.verbosity = 'minimal'
  msb.solution = Dir['**/*.sln'].first
end
