current_dir=$(dirname "$0")

dotnet pack "$current_dir/src/EnvManager.Cli/"
dotnet tool install -g --add-source "$current_dir/src/EnvManager.Cli/nupkg/" --version "0.0.1-dev" envmanager.cli --allow-downgrade
