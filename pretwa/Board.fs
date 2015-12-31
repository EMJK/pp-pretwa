namespace Pretwa
open Pretwa.Utils
open System

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

    (*let allValidMoves boardState = 
        (validMovesForColor (FieldState.Color(Player.Black)) boardState) @
        (validMovesForColor (FieldState.Color(Player.Red)) boardState)*)

    let isColor nextMove =
        match nextMove with
        | NextMove.Color(Player.Red) -> true
        | NextMove.Color(Player.Black) -> true
        | NextMove.Piece(_) -> false

    let isBlack nextMove =
        match nextMove with
        | NextMove.Color(Player.Red) -> false
        | NextMove.Color(Player.Black) -> true
        | NextMove.Piece(_) -> false

    let getField (Piece p) = p

    let allValidMoves nextMove boardState = 
        let validmoves  = 
            if isColor(nextMove) && isBlack(nextMove) 
            then (validMovesForColor (FieldState.Color(Player.Black)) boardState) 
            elif isColor(nextMove) 
            then (validMovesForColor (FieldState.Color(Player.Red)) boardState) 
            else (validMovesForField (getField nextMove) boardState)
        (validmoves)
            

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
            let nextState =
                boardState 
                |> Map.add ffrom FieldState.Empty 
                |> Map.add fto (FieldState.Color color)
                //|> (fun bs -> (bs, (NextMove.Color color)))
            let nextMove = NextMove.Color( otherPlayer color)
            (nextState, nextMove)
        | [(f1, Some f2, f3)] ->
            let nextState =
                boardState
                |> Map.add f1 FieldState.Empty
                |> Map.add f2 FieldState.Empty
                |> Map.add f3 (FieldState.Color color)
            let nextMove = if validJumpMovesForField f3 nextState <> [] then NextMove.Piece(f3) else NextMove.Color( otherPlayer color)
            (nextState, nextMove)
        | _ -> failwith "Invalid move!"

   // let getRandListElement list (rng : Random)  =
   //     List.item list (rng.Next(List.length list))

    let getFirstFromTuple tuple1 =
        match tuple1 with
        | (a, b, c) -> 
            let first = a
            (first)

    let getThirdElementFromTuple tuple2 = 
        match tuple2 with
        | (a, b, c) -> 
            let third = c
            (third)

    let hasPlayerLost color boardState =
        let pieces = allPieces color boardState
        if pieces.Length <= 3 
        then true 
        else (validMovesForColor color boardState).Length = 0

    let getPlayer =
        let nextmove :NextMove = NextMove.Color(Player.Black)
        (nextmove)

    let getColor = 
        let fieldstate = FieldState.Color(Player.Black)
        (fieldstate)

    let otherColor color = 
        match color with
        | FieldState.Color(Player.Black) -> FieldState.Color(Player.Red)
        | FieldState.Color(Player.Red) -> FieldState.Color(Player.Black)


    let colorInNextMove nextMove (boardState:BoardState) = 
        match nextMove with
        | NextMove.Color(Player.Red) -> Player.Red
        | NextMove.Color(Player.Black) -> Player.Black
        | NextMove.Piece(nextMove) -> 
            let fs = boardState.[nextMove]
            match fs with
            | FieldState.Color(fs) -> fs

    let estimateFunction color boardState = 
        let baseValue = 10
        let myPieces = (allPieces color boardState).Length * 2
        let oppositePieces = (allPieces (otherColor color) boardState).Length * 2
        let value1 = myPieces - oppositePieces
        let jumpsAvailable = validJumpMovesForColor color boardState
        let value2 = jumpsAvailable.Length * 5
        let oppositeColor = otherColor color     
        let winning = hasPlayerLost oppositeColor boardState
        let value3 = if winning then 50 else 0
        let losing = hasPlayerLost color boardState
        let estimatedValue = 
            if losing 
            then 0 
            elif color = FieldState.Color(Player.Red) then value1 + value2 + value3 + baseValue
            else ((value1 + value2 + value3 + baseValue) * (-1) + 25)
        (estimatedValue)  
  
    let getMovesList nextMove color boardState= 
        match nextMove with
            | NextMove.Color(_) ->
                (validMovesForColor color boardState)    
            | NextMove.Piece(_) ->
                let fieldcoord = getField nextMove
                (validMovesForField fieldcoord boardState)  

    let getNextSituation move boardState = 
        let ffrom = getFirstFromTuple move
        let tto = getThirdElementFromTuple move
        let nextSituation = applyMove ffrom tto boardState
        (nextSituation)   

    let rec shannonFunction boardState nextMove level = 
        let color = colorInNextMove nextMove boardState
        if level = 0 || hasPlayerLost (FieldState.Color(Player.Black)) boardState || hasPlayerLost (FieldState.Color(Player.Red)) boardState
        then (estimateFunction (FieldState.Color(color)) boardState)
        else
            let moves = getMovesList nextMove (FieldState.Color(color)) boardState
            let listOfValues = [for move in moves do
                                let nextSit = getNextSituation move boardState             
                                yield (shannonFunction (fst(nextSit)) (snd(nextSit)) (level-1))]
            if color = Player.Red
            then 
                (List.max listOfValues)
            else
                (List.min listOfValues)
        
 
    let moveOfTheComputer boardState nextMove =
        let movesComp = getMovesList nextMove (FieldState.Color(Player.Red)) boardState
        let level = 3
        let listMoves = [for move in movesComp do
                            let nextSit = getNextSituation move boardState
                            let value = shannonFunction (fst(nextSit)) (snd(nextSit)) level
                            yield (move, value)]
        let maxX = List.maxBy snd listMoves
        printfn "Lista listMmoves: %A" listMoves
        printfn "MAX: %A" maxX
        printfn ""
        let moveComp = fst maxX
        let moveFrom = getFirstFromTuple moveComp
        let moveTo = getThirdElementFromTuple moveComp
        (applyMove moveFrom moveTo boardState)

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
