rem https://github.com/StefH/GitHubReleaseNotes

SET version=0.0.1

GitHubReleaseNotes --path ../../ --output ReleaseNotes.md --skip-empty-releases --exclude-labels question invalid doc duplicate --version %version% --token %GH_TOKEN%

GitHubReleaseNotes --path ../../ --output PackageReleaseNotes.txt --skip-empty-releases --exclude-labels question invalid doc duplicate --template PackageReleaseNotes.template --version %version% --token %GH_TOKEN%