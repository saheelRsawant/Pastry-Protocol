#r "nuget: Akka.FSharp"
#r "nuget: Akka.TestKit"
module DataStructure =
    open System
    open Akka.Actor
    let mutable actorDict : Map<String, IActorRef> = Map.empty 
    let mutable actorTraverseDict: Map<String, Double list> = Map.empty
    let mutable deadActors : Set<String> = Set.empty