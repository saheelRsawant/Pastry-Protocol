#r "nuget: Akka.FSharp"
#r "nuget: Akka.TestKit"
#load "MessageTypes.fsx"
#load "AllFunctions.fsx"
#load "InitializeDS.fsx"


open Akka.FSharp
open System
open MessageTypes.Messages
open AllFunctions.Functions
open InitializeDS.DataStructure
module Peer =
    let Peer (mailBox:Actor<_>) = 
        let mutable leafSet : Set<String> = Set.empty
        let mutable routingTable: string[,] = Array2D.zeroCreate 0 0
        let mutable commonPrefixLength=0
        let mutable currRow =0
        let mutable peerId = String.Empty
        let mutable rows = 0 
        let mutable column = 16
        
        let rec loop() = actor {
            let! message = mailBox.Receive()
            match message with

                | JoinNode(key, currentIndex) ->
                    let mutable i = 0
                    let mutable k = currentIndex

                    while key.[i] = peerId.[i] do
                        i<- i+1
                    commonPrefixLength <- i
                    let mutable routingRow: string[] = Array.zeroCreate 0

                    while k<=commonPrefixLength do
                        routingRow <- get k routingTable
                        routingRow.[Int32.Parse(peerId.[commonPrefixLength].ToString(), Globalization.NumberStyles.HexNumber)] <- peerId
                        
                        let foundKey = actorDict.TryFind key
                        match foundKey with
                        | Some x->
                            x<! SetRouting(routingRow)
                            ()
                        | None -> printfn "Key does not exist in the map!"

                        k<- k+1

                    let rtrow = commonPrefixLength
                    let rtcol = Int32.Parse(key.[commonPrefixLength].ToString(), Globalization.NumberStyles.HexNumber)
                    if isNull routingTable.[rtrow, rtcol] then
                        routingTable.[rtrow, rtcol] <- key
                    else
                        let temp = routingTable.[rtrow, rtcol]
                        let final = actorDict.TryFind temp

                        match final with
                        | Some x ->
                            x<!JoinNode(key, k)
                        | None ->printfn "Key does not exist in the map "


                | Path(key, source, hops) ->
                    if peerId = key then
                        if actorTraverseDict.ContainsKey(source) then
                            let foundKey = actorTraverseDict.TryFind source
                            match foundKey with 
                            | Some x->
                                let averageHops = x.[0]
                                let finalSum = x.[1]
                                let value = [((averageHops*finalSum)+(double(hops)))/ (finalSum+1.0); finalSum+1.0]
                                actorTraverseDict <- actorTraverseDict.Add(source, value)
                            | None -> printfn "Key does not exist in the map"
                        else
                            let value = [double(hops) ; 1.0]
                            actorTraverseDict <- actorTraverseDict.Add(source, value)


                    elif leafSet.Contains(key) then
                        let actor = actorDict.Item(key)
                        actor <! Path(key, source, hops+1)

                    else
                        let mutable i = 0
                        while key.[i] = peerId.[i] do
                            i<- i+1
                        commonPrefixLength <- i
                        let mutable rtrow = commonPrefixLength
                        let mutable rtcol = Int32.Parse(key.[commonPrefixLength].ToString(), Globalization.NumberStyles.HexNumber)
                        if isNull routingTable.[rtrow, rtcol] then
                            rtcol <- 0

                        actorDict.Item(routingTable.[rtrow, rtcol]) <! Path(key, source, hops+1)

                | BuildNetwork(i,d)->
                    peerId <- i
                    rows <- d
                    routingTable <- Array2D.zeroCreate rows column
                    let number = Int32.Parse(peerId, Globalization.NumberStyles.HexNumber)

                    let mutable neighbor1 = number
                    let mutable neighbor2 = number
                    let mutable iterator = 0

                    while iterator < 8 do 
                        if neighbor1 = 0 then
                            neighbor1 <- actorDict.Count-1 
                        leafSet <- leafSet.Add(neighbor1.ToString())
                        iterator  <- iterator  + 1
                        neighbor1 <- neighbor1 - 1
                      
                    while iterator  < 16 do
                        if neighbor2 = actorDict.Count-1 then
                          neighbor2 <- 0
                        leafSet <- leafSet.Add(neighbor2.ToString())
                        iterator  <- iterator  + 1
                        neighbor2 <- neighbor2 + 1
                
                
                | SetRouting(row: String[])->
                    routingTable.[currRow , *] <- row
                    currRow  <- currRow  + 1

            return! loop()
            }
        loop()
        
