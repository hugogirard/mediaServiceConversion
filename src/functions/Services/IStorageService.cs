using System;

namespace functions.Services
{
    public interface IStorageService
    {
        Uri GetSharedAccessReferenceForUpload(string filename);
    }
}