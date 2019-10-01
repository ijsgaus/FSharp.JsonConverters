namespace FSharp.Tests.Types
open System

type TestRecord = {
    Field1 : string
    Field2 : int option 
}

type TestRecordRec = {
    Field1 : string
    Field2 : TestRecordRec option 
}

type ErasedUnion = ErasedUnion of int
type ErasedUnion1 = ErasedUnion1 of int * string
type ErasedUnion2 = ErasedUnion2 of flag: bool * name: string


type StringUnion = | Value1 | Value2

type OtherUnion =
    | Tuple of int * string
    | Single of string option
    | Named of f1 : int * f2 : string option
    | NoValue


 