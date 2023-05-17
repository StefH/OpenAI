# TextSplitter
This is a simple library to split texts (greedily). The specific usecase I had in mind when writing this is a chat bot that can only send message of at most, say, 2000 characters. Longer texts should be split over multiple messsages.

## Installation
This package requires dotNet Standard v2.1. You can install `TextSplitter` by installing the corresponding `nuget` package [from here](https://www.nuget.org/packages/TextSplitter/).

## Usage
The library is built around three different types: `ITextProvider`, `ITextPosition`, and `ITextChunk`. An `ITextProvider` can give you an `ITextPosition` that represents the starting position of its text. An `ITextPosition` can be asked for an `ITextChunk` that fits in a given number of characters. This is where the meat of the library lives. Finally, an `ITextChunk` provides its actual length and a method to write itself to a `StringWriter`.

All the functions you should usually need to use are the static members of `TextBuilder`. They allow you to construct `ITextProvider`s in different ways. For example:

```csharp
var text = TextBuilder.Text("This is a test.");

// GetSections returns the chunks of the provider as strings, good for testing purposes.
// Each section can have at most 5 characters in this example.
CollectionAssert.AreEqual(
    new[]("This", "is a", "test."),
    text.GetSections(5)
);
```

To get an idea of what the functions on `TextBuilder` do, it might be prudent to take a look at the tests in this repository. For a more elaborate example, try this:

```csharp
using static TextBuilder;

// Combine multiple texts and allow them to be separated by splits.
Separated(

    // Glue a headline to the paragraph. There can be breaks in the first and the second part
    // of the glue, but not between them.
    Glue(
        // This headline won't split anywhere, because it is atomic.
        Atomic("HEADLINE"),
        // This text will split at whitespaces. 
        Text("The paragraph.")
    ),
    // insert a mandatory linebreak here
    Atomic("\n"),

    // ensure that when any of the inner chunks is split it will still be correctly
    // wrapped in a markdown code block
    Wrap("```csharp", "```",
        // allow the code block to be split at newlines only
        SplitAt(
            c => c == '\n',
@"public class Program {
    public static void Main(string[] args) {
        Console.WriteLine(""Hello World"");
    }
}
"
        )
    )
)
```

## Contributing
Contributions are welcome as long as they are in the spirit of the library and come with some unit tests.

### Running the tests
This project is build with the usage of the `dotnet` CLI in mind. Consequently, running the tests only involves cd'ing into the `TextSplitter.Tests` directory and running `dotnet test`.