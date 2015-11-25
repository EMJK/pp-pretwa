using Pretwa.Gui.Common;

namespace Pretwa.Gui.Models
{
    public class NewGameModel : ObservableObject
    {
        private int _DifficultyLevel = 10;
        private GameMode _GameMode;

        public int DifficultyLevel
        {
            get { return _DifficultyLevel; }
            set
            {
                if (value == _DifficultyLevel) return;
                _DifficultyLevel = value;
                RaisePropertyChanged();
            }
        }

        public GameMode GameMode
        {
            get { return _GameMode; }
            set
            {
                if (value == _GameMode) return;
                _GameMode = value;
                RaisePropertyChanged();
            }
        }
    }
}
