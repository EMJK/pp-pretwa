namespace Pretwa

// Pole może być puste lub zajęte przez jeden z pionków (czarny lub czerwony)
type public Player =
    | Red
    | Black

type public FieldState =
    | Color of Player
    | Empty

// Ten rekord może być ALBO współrzędnymi (krotka dwóch int-ów) ALBO może oznaczać pole na środku planszy
type public FieldCoordinates =
    | Edge of int*int
    | Center

// Stan pola gry określany jest przez słownik położenie -> stan
type public BoardState = Map<FieldCoordinates, FieldState>

// Ruch określamy jako stan początkowy, współrzędne pionka i współrzędne kolejnych jego ruchów (przy biciu, tak jak w warcabach, może być wykonanych kilka ruchów z rzędu)
type public Move = {
    State : BoardState
    Start : FieldCoordinates
    Path : List<FieldCoordinates>
}

