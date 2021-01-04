
open System


module Messages = 
    type Message =
    |BuildNetwork of String * int
    |Path of String * String * int
    |JoinNode of String*int
    |SetRouting of String[]