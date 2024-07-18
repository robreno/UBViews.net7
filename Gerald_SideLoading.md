
#Sideloading .MET Maui App

Gerald Versluis on YouTube: 

Create a .NET MAUI Windows MSIX to Sideload Or Publish to the Microsoft Store

	https://youtu.be/FNwv_W3TtSU?t=910

#Links

Create a certificate for package signing

	https://learn.microsoft.com/en-us/windows/msix/package/create-certificate-package-signing

First Command: 
New-SelfSignedCertificate -Type Custom -Subject "CN=159B4EF4-25E0-45FD-B9EF-470F34B731B8" -KeyUsage DigitalSignature -FriendlyName "UBViews Certificate" -CertStoreLocation "Cert:\CurrentUser\My" -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")

Note: CN must match Platforms/Windows/Package.appmanifest file entry under: 
	
	<Identity Name="UBViews" Publisher="CN=RReno-UBViews" Version="0.1.0.0" />

Thumbprint                                Subject              EnhancedKeyUsageList
----------                                -------              --------------------
2565189F4FEF403D012593F201FD52B0642B3843  CN=RReno-UBViews     Code Signing

Note: Change PS path to folder you want to store or provide file path as below:
- FilePath "C:\Archive\Projects\UBViews_2023\CertificateData\certificate.pfx"

Enter commands below in order to produce the certificate.pfx file.

Second & Third Commands:

$password = ConvertTo-SecureString "P@ssW0rD!" -AsPlainText -Force 

Export-PfxCertificate -cert "Cert:\CurrentUser\My\2565189F4FEF403D012593F201FD52B0642B3843" -FilePath certificate.pfx -Password $password

MSBuidlCommand: 
	msbuild .\UBViews.Maui\UBViews.csproj /restore /t:Publish /p:TargetFramework=net7.0-windows10.0.19041.0 /p:configuration=release /p:GenerateAppxPackageOnBuild=true /p:Platform=x64

MSBuild Command:
	msbuild .\UBViews.Maui\UBViews.csproj /restore /t:Publish /p:TargetFramework=net7.0-windows10.0.19041.0 /p:configuration=debug /p:GenerateAppxPackageOnBuild=true /p:Platform=x64 /p:AppxPackageSigningEnabled=true /p:PackageCertificateThumbprint="2565189F4FEF403D012593F201FD52B0642B3843" /p:PackageCertificatePassword="P@ssW0rD!"

Install Certificate into Root store:
	1. Install certificate to LocalMachine
	2. Certificate Import Wizard -> Place all certificates in the following store -> Browse -> 
	3. Choose "Trusted Root Certification Authorities" -> next -> finish

Then Install application and run.



