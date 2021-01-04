open System
module Functions = 
    let rand = Random()
    let get i (arr:'T[,]) = arr.[i..i, *]|> Seq.cast<'T> |> Seq.toArray
    let multiply text times = String.replicate times text