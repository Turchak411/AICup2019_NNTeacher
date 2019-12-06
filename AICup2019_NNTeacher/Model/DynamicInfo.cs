
namespace AICup2019_NNTeacher.Model
{
    public struct DynamicInfo
    {
        public char _lastSymbol;
        public double _lastAnwser;

        public DynamicInfo(char lastSymbol, double lastAnwser)
        {
            _lastSymbol = lastSymbol;
            _lastAnwser = lastAnwser;
        }
    }
}
