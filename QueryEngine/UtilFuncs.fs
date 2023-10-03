namespace QueryEngine

open Models

module UtilFuncs =

    // Matching Nulls
    let CheckForNull =
        function
        | null -> None
        | v -> Some(v)

