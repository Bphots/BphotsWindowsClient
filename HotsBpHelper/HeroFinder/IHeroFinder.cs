namespace HotsBpHelper.HeroFinder
{
    public interface IHeroFinder
    {
        /// <summary>
        /// 根据ID识别英雄
        /// </summary>
        /// <param name="id">BP用ID</param>
        /// <returns>英雄名字</returns>
        /// <remarks>
        ///  ID示意图
        ///    0 1   7 8
        ///  2          9
        ///  3          10
        ///  4          11
        ///  5          12
        ///  6          13
        /// 
        ///  </remarks>
        string FindHero(int id);
    }
}