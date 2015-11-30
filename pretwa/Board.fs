namespace Pretwa
open Pretwa.Utils

module Board =
    let edgeCount = 3
    let edgeLength = 6
    let maxEdge = edgeCount - 1
    let maxField = edgeLength - 1

    let oppositeField fn = Circle.put (fn + edgeLength / 2) (0, maxField)
    let otherPlayer player =
        match player with
        | Player.Black -> Player.Red
        | Player.Red -> Player.Black

    let makeField (en, fn) =
        if en = -1 then FieldCoords.Center
        elif en < 0 then FieldCoords.Edge((abs en) - 2, oppositeField (Circle.put fn (0, maxField)))
        else FieldCoords.Edge(en, Circle.put fn (0, maxField))

    let walkUp (en, fn) = makeField (en - 1, fn)
    let walkDown (en, fn) = makeField (en + 1, fn)
    let walkLeft (en, fn) = makeField (en, fn - 1)
    let walkRight (en, fn) = makeField (en, fn + 1)

    let jumpUp (en, fn) = (makeField(en - 1, fn), makeField(en - 2, fn))
    let jumpDown (en, fn) = (makeField(en + 1, fn), makeField(en + 2, fn))
    let jumpLeft (en, fn) = (makeField(en, fn - 1), makeField(en, fn - 2))
    let jumpRight (en, fn) = (makeField(en, fn + 1), makeField(en, fn + 2))

    let insideBoard field = 
        match field with
        | FieldCoords.Center -> true
        | FieldCoords.Edge(en,ef) -> en >= 0 && en < edgeCount && ef >= 0 && ef < edgeLength

    let outOfBoard field = insideBoard field |> not
    
    let fieldState field (boardState: BoardState) =
        boardState.Item field

    let adjacentWalkFields field = // Funkcja zwraca współrzędne wszystkich pól przyległych do podanego
        if outOfBoard field then failwith "Out of board!"
        match field with
        | FieldCoords.Center -> [0..maxField] |> List.map (fun x -> FieldCoords.Edge(0,x)) // pole w środku zawsze sąsiaduje z 6-cioma polami krawędzi najbliżej środka
        | FieldCoords.Edge(en, ef) ->
            [walkUp (en, ef)] @
            [walkDown (en, ef)] @
            [walkLeft (en, ef)] @
            [walkRight (en, ef)]
            |> List.where insideBoard

    let adjacentJumpFields field =
        if outOfBoard field then failwith "Out of board!"
        match field with
        | FieldCoords.Center -> [0..maxField] |> List.map (fun x -> (FieldCoords.Edge(0,x), FieldCoords.Edge(1,x)))
        | FieldCoords.Edge(en, ef) -> 
            [jumpUp (en, ef)] @
            [jumpDown (en, ef)] @
            [jumpLeft (en, ef)] @
            [jumpRight (en, ef)]
            |> List.where (fun (f1, f2) -> insideBoard f1 && insideBoard f2)

    let allPieces color boardState =
        boardState
        |> Map.toList
        |> List.where (fun (_, state) -> state = color)
        |> List.map (fun (coords, _) -> coords)

    let validWalkMovesForField ffrom boardState =
        if outOfBoard ffrom then failwith "Out of board!"
        adjacentWalkFields ffrom
        |> List.where (fun field -> fieldState field boardState = FieldState.Empty)

    let validJumpMovesForField ffrom  boardState =
        if outOfBoard ffrom then failwith "Out of board!"
        match fieldState ffrom boardState with
        | FieldState.Empty -> []
        | FieldState.Color c0 ->
            adjacentJumpFields ffrom
            |> List.where (fun (f1, f2) ->
                fieldState f2 boardState = FieldState.Empty &&
                match fieldState f1 boardState with
                | FieldState.Empty -> false
                | FieldState.Color c1 -> c0 <> c1)

    let validMovesForField ffrom boardState =
        match validJumpMovesForField ffrom boardState with
        | [] -> validWalkMovesForField ffrom boardState |> List.map (fun fto -> (ffrom, None, fto))
        | list -> list |> List.map(fun (fjump, fto) -> (ffrom, Some fjump, fto))

    let validJumpMovesForColor color boardState =
        allPieces color boardState
        |> List.map (fun field -> (validJumpMovesForField field boardState) |> List.map(fun (mid, dst) -> (field, mid, dst)))
        |> List.concat

    let validWalkMovesForColor color boardState =
        let pieces = allPieces color boardState
        let jumpsAvailable = 
            allPieces color boardState
            |> List.exists (fun field -> 
                match validJumpMovesForField field boardState with
                | [] -> false
                | _ -> true)
        if jumpsAvailable 
            then [] 
            else 
                pieces 
                |> List.map (fun field -> (validWalkMovesForField field boardState) |> List.map (fun dst -> (field, dst)))
                |> List.concat

    let validMovesForColor color boardState = 
        let pieces = allPieces color boardState
        let walkMoves = validWalkMovesForColor color boardState
        match walkMoves with
        | [] -> 
            validJumpMovesForColor color boardState 
            |> List.map (fun (src, mid, dst) -> (src, Some mid, dst))
        | _ -> 
            walkMoves
            |> List.map(fun (src, dst) -> (src, None, dst))

    let allValidMoves boardState = 
        (validMovesForColor (FieldState.Color(Player.Black)) boardState) @
        (validMovesForColor (FieldState.Color(Player.Red)) boardState)

    let isValidMove ffrom fto boardState =
        validMovesForField ffrom boardState
        |> List.exists (fun (f1, _, f3) -> f1 = ffrom && f3 = fto)

    let applyMove ffrom fto boardState =
        let color = 
            match fieldState ffrom boardState with
            | FieldState.Color c -> c
            | Empty -> failwith "Invalid move!"
        match validMovesForField ffrom boardState |> List.where (fun (f1, _, f2) -> f2 = fto) with
        | [(f1, None, f2)] ->
            boardState 
            |> Map.add ffrom FieldState.Empty 
            |> Map.add fto (FieldState.Color color)
            |> (fun bs -> (bs, (NextMove.Color color)))
        | [(f1, Some f2, f3)] ->
            let nextState =
                boardState
                |> Map.add f1 FieldState.Empty
                |> Map.add f2 FieldState.Empty
                |> Map.add f3 (FieldState.Color color)
            let nextMove = if validJumpMovesForField f3 nextState <> [] then NextMove.Piece(f3) else NextMove.Color( otherPlayer color)
            (nextState, nextMove)
        | _ -> failwith "Invalid move!"

    let hasPlayerLost color boardState =
        let pieces = allPieces color boardState
        if pieces.Length <= 3 
        then true 
        else (validMovesForColor color boardState).Length = 0

    (************************************************************************)

    let defaultBoardState = // Funkcja generuje początkowy stan planszy
        let centerField = (FieldCoords.Center, FieldState.Empty) // tworzymy centralne pole
        let edgeNumbers = [0..maxEdge] // liczby od 0 do 2 oznaczające krawędzie
        let edgeFieldNumbers = [0..maxField] // liczby od 0 do 5 oznaczające numer pola na krawędzi
        let fieldCoordinates = cartesian edgeNumbers edgeFieldNumbers // liczymy iloczyn katrezjański powyższych zbiorów, z czego wychodzi nam zbiór par
        let edgeFields = // przekształcamy powyższy zbiór na zbiór krotek opisujących pola na planszy
            fieldCoordinates // bierzemy nasze pary...
            |> List.map (fun (en, ef) -> // wtłaczamy je do funkcji...
                if ef < edgeLength / 2 then // jeżeli pole znajduje się po lewej stronie...
                    (FieldCoords.Edge(en,ef), FieldState.Color(Player.Black)) // nadajemy mu kolor czarny
                else // natomiast jeżeli znajduje się po prawej stronie...
                    (FieldCoords.Edge(en,ef), FieldState.Color(Player.Red))) // nadajemy mu kolor czerwony
        let allFields = centerField :: edgeFields // na koniec dołączamy na początek listy pole w centrum. Wychodzi nam zbiór krotek (współrzędne, kolor)
        Map.ofList allFields // Zwracamy zbiór krotek przetworzony na słownik [współrzędne -> stan]

    (************************************************************************)
