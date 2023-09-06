namespace QueryFilter

open System
open System.IO
open System.Collections.Generic
open System.Text.RegularExpressions
open System.Xml.Linq
open System.Linq

module Say =
    let hello name =
        printfn "Hello %s" name
