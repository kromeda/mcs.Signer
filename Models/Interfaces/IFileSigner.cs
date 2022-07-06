namespace Signer.Models
{
    public interface IFileSigner
    {
        byte[] DetachedSignature(string serial, byte[] file);

        byte[] SignFile(string serial, byte[] file);
    }
}
