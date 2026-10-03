using Xunit;
using GXCodeInterpreter;

namespace GXCode.Tests;

public class ParserErrorTests
{
    [Fact]
    public void IndeterminableLine_Should_Throw()
    {
        string content = "hello";
        List<string> lines = GXCodeHelper.SplitCode(content);

        Assert.Throws<GXCIndeterminableLineError>(() =>
        {
            GXCodeRoot.Start(content, lines);
        });
    }

    [Fact]
    public void NothingToClose_Should_Throw()
    {
        string content = "}";
        List<string> lines = GXCodeHelper.SplitCode(content);

        Assert.Throws<GXCNothingToCloseError>(() =>
        {
            GXCodeRoot.Start(content, lines);
        });
    }

    [Fact]
    public void MultipleEntrypoint_Should_Throw()
    {
        string content =
        """
        entrypoint() {
        }

        entrypoint() {
        }
        """;

        List<string> lines = GXCodeHelper.SplitCode(content);

        Assert.Throws<GXCMultipleEntrypointError>(() =>
        {
            GXCodeRoot.Start(content, lines);
        });
    }

    [Fact]
    public void StrayElseIf_Should_Throw()
    {
        string content =
        """
        entrypoint() {
            else if (true) {
            }
        }
        """;

        List<string> lines = GXCodeHelper.SplitCode(content);

        Assert.Throws<GXCStrayElseIfError>(() =>
        {
            GXCodeRoot.Start(content, lines);
        });
    }

    [Fact]
    public void StrayElse_Should_Throw()
    {
        string content =
        """
        entrypoint() {
            else {
            }
        }
        """;

        List<string> lines = GXCodeHelper.SplitCode(content);

        Assert.Throws<GXCStrayElseError>(() =>
        {
            GXCodeRoot.Start(content, lines);
        });
    }

    [Fact]
    public void StrayCase_Should_Throw()
    {
        string content =
        """
        entrypoint() {
            case "abc" {
            }
        }
        """;

        List<string> lines = GXCodeHelper.SplitCode(content);

        Assert.Throws<GXCStrayCaseError>(() =>
        {
            GXCodeRoot.Start(content, lines);
        });
    }

    [Fact]
    public void NestedEntrypoint_Should_Throw()
    {
        string content =
        """
        class Test {
            init() {
                entrypoint() {
                }
            }
        }
        """;

        List<string> lines = GXCodeHelper.SplitCode(content);

        Assert.Throws<GXCNestedEntrypointError>(() =>
        {
            GXCodeRoot.Start(content, lines);
        });
    }

    [Fact]
    public void StrayIfBlock_Should_Throw()
    {
        string content =
        """
        if (true) {
        }
        """;

        List<string> lines = GXCodeHelper.SplitCode(content);

        Assert.Throws<GXCStrayBlockError>(() =>
        {
            GXCodeRoot.Start(content, lines);
        });
    }

    [Fact]
    public void StrayBuiltinOperation_Should_Throw()
    {
        string content =
        """
        out "Hello";
        """;

        List<string> lines = GXCodeHelper.SplitCode(content);

        Assert.Throws<GXCStrayBuiltinOperationError>(() =>
        {
            GXCodeRoot.Start(content, lines);
        });
    }

    [Fact]
    public void StrayVariableDeclaration_Should_Throw()
    {
        string content =
        """
        int x = 5;
        """;

        List<string> lines = GXCodeHelper.SplitCode(content);

        Assert.Throws<GXCStrayVariableDeclarationError>(() =>
        {
            GXCodeRoot.Start(content, lines);
        });
    }

    [Fact]
    public void StrayVariableAssignment_Should_Throw()
    {
        string content =
        """
        x = 5;
        """;

        List<string> lines = GXCodeHelper.SplitCode(content);

        Assert.Throws<GXCStrayVariableAssignmentError>(() =>
        {
            GXCodeRoot.Start(content, lines);
        });
    }

    [Fact]
    public void MissingEntrypoint_Should_Throw()
    {
        string content =
        """
        class Test {
            init() {
            }
        }
        """;

        List<string> lines = GXCodeHelper.SplitCode(content);

        Assert.Throws<GXCMissingEntrypointError>(() =>
        {
            GXCodeRoot.Start(content, lines);
        });
    }

    [Fact]
    public void WrongType_Should_Throw()
    {
        string content =
        """
        entrypoint() {
            int x = "hello";
        }
        """;

        List<string> lines = GXCodeHelper.SplitCode(content);

        Assert.Throws<GXCWrongTypeError>(() =>
        {
            GXCodeRoot.Start(content, lines);
        });
    }

    // GX0014 (UnsupportedType) ist mit der aktuellen Parser-Architektur
    // nicht mehr erreichbar und wird deshalb bewusst nicht getestet.

    [Fact]
    public void UndeclaredVariable_Should_Throw()
    {
        string content =
        """
        entrypoint() {
            out abc;
        }
        """;

        List<string> lines = GXCodeHelper.SplitCode(content);

        Assert.Throws<GXCUndeclaredVariableError>(() =>
        {
            GXCodeRoot.Start(content, lines);
        });
    }

    [Fact]
    public void StrayVariableArithmetic_Should_Throw()
    {
        string content =
        """
        x += 1;
        """;

        List<string> lines = GXCodeHelper.SplitCode(content);

        Assert.Throws<GXCStrayVariableArithmeticError>(() =>
        {
            GXCodeRoot.Start(content, lines);
        });
    }

    [Fact]
    public void WrongArithmetic_Should_Throw()
    {
        string content =
        """
        entrypoint() {
            str text = "abc";
            text += "def";
        }
        """;

        List<string> lines = GXCodeHelper.SplitCode(content);

        Assert.Throws<GXCWrongArithmeticError>(() =>
        {
            GXCodeRoot.Start(content, lines);
        });
    }

    [Fact]
    public void MultipleNamespace_Should_Throw()
    {
        string content =
        """
        #ns A
        #ns B

        entrypoint() {
        }
        """;

        List<string> lines = GXCodeHelper.SplitCode(content);

        Assert.Throws<GXCMultipleNamespaceError>(() =>
        {
            GXCodeRoot.Start(content, lines);
        });
    }

    [Fact]
    public void WrongNamespaceDefinition_Should_Throw()
    {
        string content =
        """
        // comment
        #ns Test

        entrypoint() {
        }
        """;

        List<string> lines = GXCodeHelper.SplitCode(content);

        Assert.Throws<GXCWrongNamespaceDefinitionError>(() =>
        {
            GXCodeRoot.Start(content, lines);
        });
    }

    [Fact]
    public void WrongClassModifier_Should_Throw()
    {
        string content =
        """
        public class Test {
            init() {
            }
        }
        """;

        List<string> lines = GXCodeHelper.SplitCode(content);

        Assert.Throws<GXCWrongClassModifierError>(() =>
        {
            GXCodeRoot.Start(content, lines);
        });
    }

    [Fact]
    public void WrongMethodModifier_Should_Throw()
    {
        string content =
        """
        class Test {
            public method Test() {
            }

            init() {
            }
        }
        """;

        List<string> lines = GXCodeHelper.SplitCode(content);

        Assert.Throws<GXCWrongMethodModifierError>(() =>
        {
            GXCodeRoot.Start(content, lines);
        });
    }

    [Fact]
    public void NestedClass_Should_Throw()
    {
        string content =
        """
        class Outer {
            class Inner {
                init() {
                }
            }

            init() {
            }
        }
        """;

        List<string> lines = GXCodeHelper.SplitCode(content);

        Assert.Throws<GXCNestedClassError>(() =>
        {
            GXCodeRoot.Start(content, lines);
        });
    }

    [Fact]
    public void WrongInstanceInitiator_Should_Throw()
    {
        string content =
        """
        class Test {
            init() {
            }
        }

        entrypoint() {
            inst<Test> t = Test();
        }
        """;

        List<string> lines = GXCodeHelper.SplitCode(content);

        Assert.Throws<GXCWrongInstanceInitiatorError>(() =>
        {
            GXCodeRoot.Start(content, lines);
        });
    }

    [Fact]
    public void ClassMissingInit_Should_Throw()
    {
        string content =
        """
        class Test {
            method Hello() {
            }
        }

        entrypoint() {
            inst<Test> test = new();
        }
        """;

        List<string> lines = GXCodeHelper.SplitCode(content);

        Assert.Throws<GXCClassMissingInitError>(() =>
        {
            GXCodeRoot.Start(content, lines);
        });
    }

    [Fact]
    public void ConstantAssignment_Should_Throw()
    {
        string content =
        """
        entrypoint() {
            const str msg = "Hi";
            msg = "Hello";
        }
        """;

        List<string> lines = GXCodeHelper.SplitCode(content);

        Assert.Throws<GXCConstantAssignmentError>(() =>
        {
            GXCodeRoot.Start(content, lines);
        });
    }
}