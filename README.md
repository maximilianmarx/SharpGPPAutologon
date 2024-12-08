Extracting Autologon credentials within Group Policy Preferences.

### How it works
The program searches for and extracts autologon credentials from the registry.xml files found in the SYSVOL directory of the current Windows domain.
It's basically splitted into three steps:

1. **Locating SYSVOL**: It first queries the environment to get the domain's Fully Qualified Domain Name (FQDN) and constructs the UNC path for the SYSVOL directory.
2. **Searching for registry.xml Files**: Using recursive directory searching, it finds all `registry.xml` files under `\\<domain>\SYSVOL\<domain>\Policies`.
3. **Extracting Credentials**: For each `registry.xml` file, it looks for `DefaultUserName` and `DefaultPassword` values and extracts them.

### How to run
Build the project, then just run:

```cmd
.\SharpGPPAutologon.exe
```
