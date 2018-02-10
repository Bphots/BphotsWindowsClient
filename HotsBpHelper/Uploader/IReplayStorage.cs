using System.Collections.Generic;

namespace HotsBpHelper.Uploader
{
    public interface IReplayStorage
    {
        void Save(IEnumerable<ReplayFile> files);

        IEnumerable<ReplayFile> Load();
    }
}
