using Newtonsoft.Json;
using Signer.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace Signer.Services
{
    public class CryptoProSigner : IFileSigner
    {
        private X509Certificate2 selectedCert;

        public byte[] DetachedSignature(string serial, byte[] fileArray)
        {
            try
            {
                SelectCertificate(serial);

                ContentInfo contentInfo = new ContentInfo(fileArray);
                SignedCms signedCms = new SignedCms(contentInfo, true);
                CmsSigner cmsSigner = new CmsSigner(selectedCert);
                signedCms.ComputeSignature(cmsSigner);

                byte[] encodedSignature = signedCms.Encode();
                VerifyMessage(fileArray, encodedSignature);

                return encodedSignature;
            }
            catch (CryptographicException cx)
            {
                string data = $"fileLength: {fileArray.Length}{Environment.NewLine}";
                string certJson = JsonConvert.SerializeObject(selectedCert);
                data += $"selectedCert: {certJson}{Environment.NewLine}";
                throw new CryptographicException(data, cx);
            }
        }

        public byte[] SignFile(string serial, byte[] fileArray)
        {
            try
            {
                SelectCertificate(serial);

                ContentInfo contentInfo = new ContentInfo(fileArray);
                SignedCms signedCms = new SignedCms(contentInfo, false);
                CmsSigner cmsSigner = new CmsSigner(selectedCert);

                signedCms.ComputeSignature(cmsSigner);
                byte[] signedMsg = signedCms.Encode();
                VerifyFile(signedMsg);

                return signedMsg;
            }
            catch (CryptographicException cx)
            {
                string data = $"fileLength: {fileArray.Length}{Environment.NewLine}";
                string certJson = JsonConvert.SerializeObject(selectedCert);
                data += $"selectedCert: {certJson}{Environment.NewLine}";
                throw new CryptographicException(data, cx);
            }
        }

        private void SelectCertificate(string certificateSerialNumber)
        {
            X509Store store = new X509Store("MY", StoreLocation.CurrentUser);

            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            selectedCert = store.Certificates.
                Cast<X509Certificate2>().
                FirstOrDefault(certificate => certificate.SerialNumber.Equals(certificateSerialNumber, StringComparison.OrdinalIgnoreCase));

            store.Close();

            if (selectedCert == null)
            {
                throw new CryptographicException($"Сертификат с серийным номером \'{certificateSerialNumber}\' не найден.");
            }

            AsymmetricAlgorithm asym = selectedCert.PrivateKey;

            if (asym == null)
            {
                throw new CryptographicException("Нет секретного ключа соответствующего искомому сертификату.");
            }
        }

        private void VerifyMessage(byte[] message, byte[] encodedSignature)
        {
            ContentInfo contentInfo = new ContentInfo(message);
            SignedCms signedCms = new SignedCms(contentInfo, true);
            signedCms.Decode(encodedSignature);
            signedCms.CheckSignature(true);
        }

        private void VerifyFile(byte[] encodedSignedCms)
        {
            SignedCms signedCms = new SignedCms();
            signedCms.Decode(encodedSignedCms);
            signedCms.CheckSignature(false);
        }
    }
}