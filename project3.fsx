#r "nuget: Akka.FSharp"
#r "nuget: Akka.TestKit"
#load "MessageTypes.fsx"
#load "AllFunctions.fsx"
#load "InitializeDS.fsx"
#load "Peer.fsx"

open Akka.Actor
open Akka.FSharp
open System
open System.Threading
open MessageTypes.Messages
open AllFunctions.Functions
open InitializeDS.DataStructure
open Peer.Peer

let system = ActorSystem.Create("DOSProject3")
let args : string array = fsi.CommandLineArgs |> Array.tail
let mutable numNodes =  args.[0] |> int
let numRequest = args.[1] |> int
let numDigits = Math.Log(numNodes |> float, 16.0) |> ceil |> int


let mutable hexNumber = String.Empty
let mutable size = 0
let mutable peerId = String.Empty
peerId <- multiply "0" numDigits


let mutable bossActor = spawn system peerId Peer
bossActor <! BuildNetwork(peerId, numDigits)
actorDict<- actorDict.Add(peerId, bossActor)


for i in [1.. numNodes-1] do
    hexNumber <- i.ToString("X")
    size <- hexNumber.Length
    peerId <-  multiply "0" (numDigits-size) + hexNumber
    bossActor<- spawn system peerId Peer
    bossActor <! BuildNetwork(peerId, numDigits)
    actorDict<- actorDict.Add(peerId, bossActor)
    let temp = multiply "0" numDigits
    let final = actorDict.Item temp
    final<!JoinNode(peerId, 0)
    Thread.Sleep 5
 

Thread.Sleep 1000


let mutable destinationId = String.Empty
let mutable counter = 0
let mutable i = 1

let tempArray = actorDict |> Map.toSeq |> Seq.map fst |> Seq.toArray

while i<=numRequest do
    for sourceId in tempArray do
        counter <- counter + 1
        destinationId <- sourceId
        while destinationId = sourceId do
            destinationId <-  tempArray.[rand.Next tempArray.Length]
        let temp = actorDict.Item sourceId
        temp<!Path(destinationId, sourceId, 0)
        Thread.Sleep 5
    printfn "%i Request performed by all peers" i
    i<- i + 1

Thread.Sleep 1000
let mutable totalHopSize = 0.0 |> double
let avgHops = actorTraverseDict |> Map.toSeq |> Seq.map snd |> Seq.toArray 
for x in avgHops do
    totalHopSize <- totalHopSize + x.[0]
let ans = totalHopSize / double(actorTraverseDict.Count) 
printfn "Average number of Hops %f" ans
Environment.Exit 0

