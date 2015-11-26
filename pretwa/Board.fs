namespace Pretwa
open Pretwa.Utils

module Board =
    let edgeCount = 3
    let edgeLength = 6

    let cartesian set1 set2 = 
        [for x in set1 do // pętla...
            for y in set2 do // w pętli
                yield (x,y)] // zwracamy wszystkie możliwe pary

    let defaultBoardState =
        let centerField = (FieldCoordinates.Center, FieldState.Empty) // tworzymy centralne pole
        let edgeNumbers = [0..edgeCount - 1] // liczby od 0 do 2 oznaczające krawędzie
        let edgeFieldNumbers = [0..edgeLength-1] // liczby od 0 do 5 oznaczające numer pola na krawędzi
        let fieldCoordinates = cartesian edgeNumbers edgeFieldNumbers // liczymy iloczyn katrezjański powyższych zbiorów, z czego wychodzi nam zbiór par
        let edgeFields = // przekształcamy powyższy zbiór na zbiór krotek opisujących pola na planszy
            fieldCoordinates // bierzemy nasze pary...
            |> List.map (fun (en, ef) -> // wtłaczamy je do funkcji...
                if ef < edgeLength / 2 then // jeżeli pole znajduje się po lewej stronie...
                    (FieldCoordinates.Edge(en,ef), FieldState.Color(Player.Black)) // nadajemy mu kolor czarny
                else // natomiast jeżeli znajduje się po prawej stronie...
                    (FieldCoordinates.Edge(en,ef), FieldState.Color(Player.Black))) // nadajemy mu kolor czerwony
        let allFields = centerField :: edgeFields // na koniec dołączamy na początek listy pole w centrum. Wychodzi nam zbiór krotek (współrzędne, kolor)
        Map.ofList allFields

    let outOfField coords =
        match coords with
        | FieldCoordinates.Center -> false
        | FieldCoordinates.Edge(en,ef) -> en < 0 || en >= edgeCount || ef < 0 || ef >= edgeLength

    let possibleMovesForField field state = 0

    let possibleMovesForPlayer player state = 0


    let canMove ffrom fto =
        match (ffrom, fto) with
        | (_,_) when outOfField ffrom || outOfField fto -> failwith ("Out of board!") // jeśli którekolwiek z podanych pól jest poza zakresem planszy, wywalamy błąd
        | (FieldCoordinates.Center, FieldCoordinates.Edge(0,_)) -> true // przejście z centrum na krawędź najbliżej środka jest możliwe
        | (FieldCoordinates.Edge(0,_), FieldCoordinates.Center) -> true // analogicznie przejście z krawędzi najbliżej środka do centrum też jest możliwe
        | (FieldCoordinates.Edge(en1, ef1), FieldCoordinates.Edge(en2, ef2)) -> // w pozostałych przypadkach sprawdzamy czy:
            (en1 = en2 && oneof [1;edgeLength - 1] (abs ef1-ef2)) || // zmienił się tylko indeks pola o 1, indeks krawędzi ten sam
            (ef1 = ef2 && (abs en1-en2) = 1) // zmienił się indeks krawędzi o 1, indeks pola bez zmian
        | _ -> false // we wszystkich innych przypadkach ruch nie jest możliwy

