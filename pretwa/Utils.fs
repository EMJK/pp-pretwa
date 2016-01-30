namespace Pretwa
module Utils =
    let oneof list item = List.exists(fun x -> x = item) list

    let cartesian set1 set2 =
        [for x in set1 do // pętla...
            for y in set2 do // w pętli
                yield (x,y)] // zwracamy wszystkie możliwe pary

    module Circle =
        let put value (cMin, cMax) =
            let offset = cMin
            let tmpMax = cMax - offset
            let count = cMax - cMin + 1
            let tmpValue = value - offset
            offset +
                if tmpValue < 0
                then tmpMax - abs(tmpValue % count) + 1
                else tmpValue % count

        let up value (cMin, cMax) = put (value + 1) (cMin, cMax)

        let down value (cMin, cMax) = put (value - 1) (cMin, cMax)

        let distance val1 val2 (cMin, cMax) =
            let p1 = put val1 (cMin, cMax)
            let p2 = put val2 (cMin, cMax)
            let dist = abs(p1-p2)
            let count = abs(cMin-cMax) + 1
            min(dist, count - dist)

    let mapMerge group1 group2 appender =
        group1 |> Seq.fold(fun (acc:Map<'a,'b>) (KeyValue(key, values)) ->
            match acc.TryFind key with
            | Some items -> Map.add key (appender values items) acc
            | None -> Map.add key values acc) group2

    let joinMaps map1 map2 = mapMerge map1 map2 Seq.append