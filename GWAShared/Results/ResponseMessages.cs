using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWAShared.Results
{
    public static class ResponseMessages
    {
        public const string DataSaved = "Data has been saved successfully.";
        public const string DataUpdated = "Data has been updated successfully.";
        public const string DataDeleted = "Data has been deleted successfully.";
        public const string DataSaveFailed = "Failed to save data.";
        public const string DataUpdateFailed = "Failed to update data.";
        public const string DataDeleteFailed = "Failed to delete data.";
        public const string InternalServerError = "Internal server error.";
        public const string DataFound = "Data Found.";
        public const string DataNotFound = "Data Not Found.";
    }
}
