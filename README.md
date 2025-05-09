Digital Signatures in .NET Core 9
A digital signature is a cryptographic technique that provides:

Authentication (verifies the signer's identity)

Integrity (ensures the document hasn't been altered)

Non-repudiation (signer cannot deny signing)

Prerequisites:

Required NuGet Packages:

bash
dotnet add package itext7 --version 8.0.3
dotnet add package BouncyCastle.Cryptography --version 2.2.1
Create a test PDF (original.pdf):

Create a simple PDF document to use as input

Key Improvements:

Certificate Generation:

The code includes a self-signed certificate generator

Creates a valid X.509 certificate with proper RSA key pair

Exports to PFX with password protection

Error Handling:

Proper exception handling

Checks for certificate validity

Validates private key existence

.NET 9 Compatibility:

Uses modern async/await patterns

Implements IDisposable properly

Uses latest cryptography APIs

Verification Process:

Proper signature integrity checks

Validates all signatures in document

Returns overall verification status

To Run:

Create a new .NET 9 Console App:

bash
dotnet new console -n PdfSigner -f net9.0
Add the required packages

Place the test PDF (original.pdf) in the project directory

Run the program:

bash
dotnet run
Troubleshooting Tips:

If you get "File not found" errors:

Verify file paths are correct

Check file permissions

If you get certificate errors:

Delete signature.pfx and let the program regenerate it

Try importing the certificate to your trusted root store

For signature verification failures:

Ensure you're not modifying the signed PDF

Check the certificate hasn't expired

Important Notes:

Production Use:

Replace self-signed certificate with one from a trusted CA

Add timestamp server support

Implement proper certificate revocation checks

Security:

Never hardcode passwords in production code

Use secure storage for certificates

Rotate certificates regularly

This implementation provides a complete workflow from certificate generation to PDF signing and verification. The code handles common edge cases and includes proper resource management for .NET 9.
