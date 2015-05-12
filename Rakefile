require 'bundler/setup'

require 'albacore'
require 'albacore/tasks/versionizer'
require 'albacore/ext/teamcity'

Configuration = ENV['CONFIGURATION'] || 'Debug'

Albacore::Tasks::Versionizer.new :versioning

desc 'create assembly infos'
asmver_files :assembly_info do |a|
  a.files = FileList['**/*proj'] # optional, will find all projects recursively by default

  a.attributes assembly_description: 'TODO',
               assembly_configuration: Configuration,
               assembly_company: '',
               assembly_copyright: "(c) 2015 by Mattias Jakobsson",
               assembly_version: ENV['LONG_VERSION'],
               assembly_file_version: ENV['LONG_VERSION'],
               assembly_informational_version: ENV['BUILD_VERSION']
end

asmver :asmver do |a|
  a.file_path  = 'src/CommonAssemblyInfo.cs'
  a.attributes assembly_version: ENV['LONG_VERSION'],
    assembly_file_version: ENV['LONG_VERSION']
  a.using 'System'
end

desc 'Perform fast build (warn: doesn\'t d/l deps)'
build :quick_compile do |b|
  b.prop 'Configuration', Configuration
  b.logging = 'detailed'
  b.sln     = 'src/Jajo.Web.sln'
end

task :paket_bootstrap do
system 'tools/paket.bootstrapper.exe', clr_command: true unless   File.exists? 'tools/paket.exe'
end

desc 'restore all nugets as per the packages.config files'
task :restore => :paket_bootstrap do
  system 'tools/paket.exe', 'restore', clr_command: true
end

desc 'Perform full build'
build :compile => [:versioning, :restore, :asmver] do |b|
  b.prop 'Configuration', Configuration
  b.sln = 'src/Jajo.Web.sln'
end

directory 'build/pkg'

desc 'package nugets - finds all projects and package them'
nugets_pack :create_nugets => [:versioning] do |p|
  FileUtils.rm_rf(Dir.glob('build/pkg/*'))
  system 'tools/paket.exe', 'pack', 'output', 'build/pkg', 'version', ENV['BUILD_VERSION']
end

namespace :tests do
  #task :unit do
  #  system "src/MyProj.Tests/bin/#{Configuration}/MyProj.Tests.exe", clr_command: true
  #end
end

# task :tests => :'tests:unit'

task :default => :compile

task :ci => [:restore, :default, :create_nugets]

#task :ensure_nuget_key do
#  raise 'missing env NUGET_KEY value' unless ENV['NUGET_KEY']
#end

#Albacore::Tasks::Release.new :release,
#                             pkg_dir: 'build/pkg',
#                             depend_on: [:create_nugets, :ensure_nuget_key],
#                             nuget_exe: 'packages/NuGet.CommandLine/tools/NuGet.exe',
#                             api_key: ENV['NUGET_KEY']
