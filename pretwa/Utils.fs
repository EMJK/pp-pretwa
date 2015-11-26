namespace Pretwa
module Utils =
    let oneof list item = List.exists(fun x -> x = item) list