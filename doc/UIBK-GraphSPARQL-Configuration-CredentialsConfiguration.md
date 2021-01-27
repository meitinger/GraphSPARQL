#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Configuration](./UIBK-GraphSPARQL-Configuration.md 'UIBK.GraphSPARQL.Configuration')
## CredentialsConfiguration Class
Configuration element for credentials.  
```csharp
public sealed class CredentialsConfiguration : UIBK.GraphSPARQL.Configuration.JsonElement,
System.Net.ICredentials,
System.Net.ICredentialsByHost
```
Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [JsonElement](./UIBK-GraphSPARQL-Configuration-JsonElement.md 'UIBK.GraphSPARQL.Configuration.JsonElement') &#129106; CredentialsConfiguration  

Implements [System.Net.ICredentials](https://docs.microsoft.com/en-us/dotnet/api/System.Net.ICredentials 'System.Net.ICredentials'), [System.Net.ICredentialsByHost](https://docs.microsoft.com/en-us/dotnet/api/System.Net.ICredentialsByHost 'System.Net.ICredentialsByHost')  
### Properties
- [Domain](./UIBK-GraphSPARQL-Configuration-CredentialsConfiguration-Domain.md 'UIBK.GraphSPARQL.Configuration.CredentialsConfiguration.Domain')
- [Password](./UIBK-GraphSPARQL-Configuration-CredentialsConfiguration-Password.md 'UIBK.GraphSPARQL.Configuration.CredentialsConfiguration.Password')
- [UseDefault](./UIBK-GraphSPARQL-Configuration-CredentialsConfiguration-UseDefault.md 'UIBK.GraphSPARQL.Configuration.CredentialsConfiguration.UseDefault')
- [UserName](./UIBK-GraphSPARQL-Configuration-CredentialsConfiguration-UserName.md 'UIBK.GraphSPARQL.Configuration.CredentialsConfiguration.UserName')
### Methods
- [JsonInitialize()](./UIBK-GraphSPARQL-Configuration-CredentialsConfiguration-JsonInitialize().md 'UIBK.GraphSPARQL.Configuration.CredentialsConfiguration.JsonInitialize()')
### Operators
- [implicit operator NetworkCredential(UIBK.GraphSPARQL.Configuration.CredentialsConfiguration)](./UIBK-GraphSPARQL-Configuration-CredentialsConfiguration-op_ImplicitSystem-Net-NetworkCredential(UIBK-GraphSPARQL-Configuration-CredentialsConfiguration).md 'UIBK.GraphSPARQL.Configuration.CredentialsConfiguration.op_Implicit System.Net.NetworkCredential(UIBK.GraphSPARQL.Configuration.CredentialsConfiguration)')
