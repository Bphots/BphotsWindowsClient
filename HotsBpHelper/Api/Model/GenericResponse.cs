using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotsBpHelper.Uploader;

namespace HotsBpHelper.Api.Model
{
    public class GenericResponse
    {
        public bool Exists { get; set; }

        public bool Success { get; set; }

        public string Status { get; set; }
    }
}
