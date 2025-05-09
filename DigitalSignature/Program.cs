using iText.Bouncycastle.X509;
using iText.Commons.Bouncycastle.Cert;
using iText.Kernel.Pdf;
using iText.Signatures;
using Org.BouncyCastle.X509;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace DigitalSignature;
class Program
{
    static void Main(string[] args)
    {
        try
        {
            if (!File.Exists("signature.pfx"))
            {
                GenerateTestCertificate();
            }

            SignPdf("original.pdf", "signed.pdf", "signature.pfx", "password");
            Console.WriteLine("PDF signed successfully!");

            Console.WriteLine($"Signature valid: {VerifySignature("signed.pdf")}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static void SignPdf(string inputFile, string outputFile, string certPath, string password)
    {
        using var cert = new X509Certificate2(certPath, password, X509KeyStorageFlags.Exportable);

        var parser = new X509CertificateParser();
        var bcCert = parser.ReadCertificate(cert.GetRawCertData());
        var iTextCert = new X509CertificateBC(bcCert);
        var chain = new IX509Certificate[] { iTextCert };

        using var reader = new PdfReader(inputFile);
        using var writer = new FileStream(outputFile, FileMode.Create);

        var signer = new PdfSigner(reader, writer, new StampingProperties());

        // Updated to use the public method for setting signature appearance
        var appearance = signer.GetSignerProperties()
            .SetReason("Test Signature")
            .SetLocation("Virtual Office")
            .SetPageRect(new iText.Kernel.Geom.Rectangle(100, 100, 200, 100))
            .SetPageNumber(1);

        IExternalSignature signature = new X509Certificate2SignatureWrapper(cert, "SHA-256");

        signer.SignDetached(
            signature,
            chain,
            null,
            null,
            null,
            0,
            PdfSigner.CryptoStandard.CMS
        );
    }

    static bool VerifySignature(string pdfPath)
    {
        using var pdfDoc = new PdfDocument(new PdfReader(pdfPath));
        var signUtil = new SignatureUtil(pdfDoc);

        foreach (string name in signUtil.GetSignatureNames())
        {
            var pkcs7 = signUtil.ReadSignatureData(name);
            if (!pkcs7.VerifySignatureIntegrityAndAuthenticity())
            {
                return false;
            }
        }
        return true;
    }

    static void GenerateTestCertificate()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            "CN=Test Certificate, OU=Development, O=My Company, C=US",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1
        );

        request.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(false, false, 0, true));

        request.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.NonRepudiation,
                true));

        var certificate = request.CreateSelfSigned(
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddYears(1)
        );

        File.WriteAllBytes("signature.pfx",
            certificate.Export(X509ContentType.Pfx, "password"));
    }
}

public class X509Certificate2SignatureWrapper : IExternalSignature
{
    private readonly X509Certificate2 _cert;
    private readonly string _hashAlgorithm;

    public X509Certificate2SignatureWrapper(X509Certificate2 cert, string hashAlgorithm)
    {
        _cert = cert ?? throw new ArgumentNullException(nameof(cert));
        _hashAlgorithm = hashAlgorithm ?? throw new ArgumentNullException(nameof(hashAlgorithm));
    }

    public string GetHashAlgorithm() => _hashAlgorithm;
    public string GetEncryptionAlgorithm() => "RSA";

    public string GetDigestAlgorithmName() => _hashAlgorithm;
    public string GetSignatureAlgorithmName() => "RSA";
    public ISignatureMechanismParams GetSignatureMechanismParameters() => null;

    public byte[] Sign(byte[] message)
    {
        using var privateKey = _cert.GetRSAPrivateKey()
            ?? throw new InvalidOperationException("No private key available");

        return privateKey.SignData(
            message,
            GetHashAlgorithmName(),
            RSASignaturePadding.Pkcs1
        );
    }

    private HashAlgorithmName GetHashAlgorithmName() => _hashAlgorithm switch
    {
        "SHA-256" => HashAlgorithmName.SHA256,
        "SHA-384" => HashAlgorithmName.SHA384,
        "SHA-512" => HashAlgorithmName.SHA512,
        _ => throw new NotSupportedException($"Hash algorithm {_hashAlgorithm} not supported")
    };
}