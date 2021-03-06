namespace Quibble.FSharp.UnitTests

module JsonStringsTests = 

    open Xunit
    open Quibble

    [<Fact>]
    let ``true = true``() = 
        let diffs = JsonStrings.verify "true" "true"
        Assert.Empty(diffs)

    [<Fact>]
    let ``false = false``() = 
        let diffs = JsonStrings.verify "false" "false"
        Assert.Empty(diffs)

    [<Fact>]
    let ``0 = 0``() = 
        let diffs = JsonStrings.verify "0" "0"
        Assert.Empty(diffs)

    [<Fact>]
    let ``0 = -0``() = 
        let diffs = JsonStrings.verify "0" "-0"
        Assert.Empty(diffs)
        
    [<Fact>]
    let ``1000 = 1E3``() = 
        let diffs = JsonStrings.verify "1000" "1E3"
        Assert.Empty(diffs)
        
    [<Fact>]
    let ``123.4 = 1.234E2``() = 
        let diffs = JsonStrings.verify "123.4" "1.234E2"
        Assert.Empty(diffs)

    [<Fact>]
    let ``1 != 2 : number value mismatch``() = 
        let diffMessage = JsonStrings.verify "1" "2" |> List.head
        Assert.Equal("Number value mismatch at $.\nExpected 2 but was 1.", diffMessage)
        
    [<Fact>]
    let ``1 != 1E1 : number value mismatch``() = 
        let diffMessage = JsonStrings.verify "1" "1E1" |> List.head
        Assert.Equal("Number value mismatch at $.\nExpected 1E1 but was 1.", diffMessage)
        
    [<Fact>]
    let ``1 = 1.0``() =
        let diffs = JsonStrings.verify "1" "1.0"
        Assert.Empty(diffs)

    [<Fact>]
    let ``true != 1 : Kind mismatch``() =
        let diffs = JsonStrings.verify "true" "1"
        match diffs with
        | [ diff ] -> Assert.Equal("Kind mismatch at $.\nExpected the number 1 but was the boolean true.", diff)
        | _ -> failwith "Wrong number of diffs"

    [<Fact>]
    let ``"foo" = "foo"``() = 
        let diffs = JsonStrings.verify "\"foo\"" "\"foo\""
        Assert.Empty(diffs)

    [<Fact>]
    let ``"foo" != "bar"``() = 
        let diffs = JsonStrings.verify "\"foo\"" "\"bar\""
        match diffs with
        | [ diff ] -> Assert.Equal("String value mismatch at $.\nExpected bar but was foo.", diff)
        | _ -> failwithf "Expected 1 diff but was %d." (List.length diffs)
        
    [<Fact>]
    let ``null vs 1``() = 
        let diffMessage = JsonStrings.verify "null" "1" |> List.head
        Assert.Equal("Kind mismatch at $.\nExpected the number 1 but was null.", diffMessage)

    [<Fact>]
    let ``Empty array vs null``() = 
        let diffMessage = JsonStrings.verify "[]" "null" |> List.head
        Assert.Equal("Kind mismatch at $.\nExpected null but was an empty array.", diffMessage)
            
    [<Fact>]
    let ``Empty array vs empty object``() = 
        let diffs = JsonStrings.verify "[]" "{}"
        match diffs with
        | [ diff ] -> Assert.Equal("Kind mismatch at $.\nExpected an object but was an empty array.", diff)
        | _ -> failwith "Wrong number of diffs"

    [<Fact>]
    let ``[ 1 ] vs [ 2 ]``() = 
        let diffs = JsonStrings.verify "[ 1 ]" "[ 2 ]"
        match diffs with
        | [ diff ] -> Assert.Equal("Number value mismatch at $[0].\nExpected 2 but was 1.", diff)
        | _ -> failwith "Wrong number of diffs"

    [<Fact>]
    let ``[ 1 ] vs [ 1, 2 ]``() =
        let diffs = JsonStrings.verify "[ 1 ]" "[ 1, 2 ]"
        match diffs with 
        | [ diffMessage ] -> Assert.Equal("Array length mismatch at $.\nExpected 2 items but was 1.", diffMessage)
        | _ -> failwith "Wrong number of diffs"

    [<Fact>]
    let ``[ 1 ] != [ 2, 1 ] : array length mismatch``() = 
        let diffs = JsonStrings.verify "[ 1 ]" "[ 2, 1 ]"
        match diffs with
        | [ diffMessage ] -> Assert.Equal("Array length mismatch at $.\nExpected 2 items but was 1.", diffMessage)
        | _ -> failwith "Wrong number of diffs"
            
    [<Fact>]
    let ``[ 2, 1 ] vs [ 1, 2 ]``() =
        let diffs = JsonStrings.verify "[ 2, 1 ]" "[ 1, 2 ]"
        match diffs with
        | [ diff1; diff2 ] ->
            Assert.Equal("Number value mismatch at $[0].\nExpected 1 but was 2.", diff1)        
            Assert.Equal("Number value mismatch at $[1].\nExpected 2 but was 1.", diff2)
        | _ -> failwithf "Expected 2 diffs but was %d." (List.length diffs)
        
    [<Fact>]
    let ``{} != { "count": 0 }``() =
        let diffs = JsonStrings.verify "{}" "{ \"count\": 0 }"
        match diffs with
        | [ diff ] ->
            Assert.Equal("Object mismatch at $.\nMissing property:\ncount (number).", diff)        
        | _ -> failwithf "Expected 1 diff but got %d." (List.length diffs)

    [<Fact>]
    let ``{ "count": 0 } != {}``() =
        let diffs = JsonStrings.verify "{ \"count\": 0 }" "{}"
        match diffs with
        | [ diff ] ->
            Assert.Equal("Object mismatch at $.\nAdditional property:\ncount (number).", diff)        
        | _ -> failwithf "Expected 1 diff but was %d." (List.length diffs)
        
    [<Fact>]
    let ``{ "age": 20, "name": "Don" } = { "name": "Don", "age": 20 }``() =
        let diffs = JsonStrings.verify """{ "age": 20, "name": "Don" }""" """{ "name": "Don", "age": 20 }"""
        Assert.Empty(diffs)
        
    [<Fact>]
    let ``Compare object with array``() =
        let s1 = "{ \"my array\": [ 1, 2, 3 ] }"
        let s2 = "{ \"my array\": [ 1, 2, 4 ] }"
        let diffs = JsonStrings.verify s1 s2
        match diffs with
        | [ diff ] ->
            Assert.Equal("Number value mismatch at $['my array'][2].\nExpected 4 but was 3.", diff)        
        | _ -> failwithf "Expected 1 diff but was %d." (List.length diffs)

