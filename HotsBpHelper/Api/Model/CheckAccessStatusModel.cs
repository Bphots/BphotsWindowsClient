using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotsBpHelper.Api.Model
{
    public class FingerPrintStatusCollection
    {
        public List<FingerPrintInfo> Status { get; set; }
    }

    public class FingerPrintInfo
    {
        public string FingerPrint { get; set; }

        public FingerPrintStatus Access { get; set; }
    }

    public enum FingerPrintStatus
    {
        Allowed,
        Reserved,
        Duplicated
    }
}
