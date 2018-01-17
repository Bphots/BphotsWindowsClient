using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HotsBpHelper.Api;
using HotsBpHelper.Api.Model;
using ImageProcessor.Ocr;

namespace HotsBpHelper.Utils.ComboBoxItemUtil
{
    public class HeroItemUtil : IComboxItemUtil
    {
        private readonly IRestApi _restApi;

        private IEnumerable<ComboBoxItemInfo> _heroInfos;

        public HeroItemUtil(IRestApi restApi)
        {
            _restApi = restApi;
        }

        public IEnumerable<ComboBoxItemInfo> GetComboxItemInfos()
        {
            if (_heroInfos == null)
            {
                _heroInfos = _restApi.GetHeroList(App.Language)
                    .Select(hi => new ComboBoxItemInfo()
                    {
                        Id = hi.Id.ToString(),
                        Name = hi.Name,
                        Acronym = hi.Acronym,
                    })
                    .OrderBy(item => item.Name);

                foreach (var heroInfo in _heroInfos)
                {
                    OcrEngine.CandidateHeroes.Add(heroInfo.Name);
                }
            }
            return _heroInfos;
        }
    }
}