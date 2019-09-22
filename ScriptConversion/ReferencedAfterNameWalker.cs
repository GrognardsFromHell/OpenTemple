using System.Collections.Generic;
using System.Linq;
using IronPython.Compiler.Ast;
using IronPython.Modules;

namespace ScriptConversion
{
    /// <summary>
    /// Searches for all referenced names AFTER a given statement.
    /// </summary>
    public class ReferencedAfterNameWalker : PythonWalker
    {
        private readonly Statement _startAfter;

        public ISet<string> ReferencedNames { get; } = new HashSet<string>();

        private bool _startStatementEncountered;

        private ISet<string> _definedInThisScope = new HashSet<string>();

        private readonly List<ISet<string>> _definitionStack = new List<ISet<string>>();

        private void EnterScope()
        {
            _definitionStack.Add(new HashSet<string>(_definedInThisScope));
        }

        private void ExitScope()
        {
            _definedInThisScope = _definitionStack[^1];
            _definitionStack.RemoveAt(_definitionStack.Count - 1);
        }

        public ReferencedAfterNameWalker(Statement startAfter)
        {
            _startAfter = startAfter;
        }

        public override void PostWalk(AssertStatement node)
        {
            if (node == _startAfter)
            {
                _startStatementEncountered = true;
            }
        }

        public override bool Walk(AssignmentStatement node)
        {
            if (_startStatementEncountered)
            {
                foreach (var expression in node.Left)
                {
                    if (expression is NameExpression nameExpression)
                    {
                        if (!ReferencedNames.Contains(nameExpression.Name))
                        {
                            _definedInThisScope.Add(nameExpression.Name);
                        }
                    }
                }
            }


            return true;
        }

        public override void PostWalk(AssignmentStatement node)
        {
            if (node == _startAfter)
            {
                _startStatementEncountered = true;
            }
        }

        public override void PostWalk(AugmentedAssignStatement node)
        {
            if (node == _startAfter)
            {
                _startStatementEncountered = true;
            }
        }

        public override void PostWalk(BreakStatement node)
        {
            if (node == _startAfter)
            {
                _startStatementEncountered = true;
            }
        }

        public override void PostWalk(ClassDefinition node)
        {
            if (node == _startAfter)
            {
                _startStatementEncountered = true;
            }
        }

        public override void PostWalk(ContinueStatement node)
        {
            if (node == _startAfter)
            {
                _startStatementEncountered = true;
            }
        }

        public override void PostWalk(DelStatement node)
        {
            if (node == _startAfter)
            {
                _startStatementEncountered = true;
            }
        }

        public override void PostWalk(EmptyStatement node)
        {
            if (node == _startAfter)
            {
                _startStatementEncountered = true;
            }
        }

        public override void PostWalk(ExecStatement node)
        {
            if (node == _startAfter)
            {
                _startStatementEncountered = true;
            }
        }

        public override void PostWalk(ExpressionStatement node)
        {
            if (node == _startAfter)
            {
                _startStatementEncountered = true;
            }
        }

        public override void PostWalk(ForStatement node)
        {
            if (node == _startAfter)
            {
                _startStatementEncountered = true;
            }
        }

        public override void PostWalk(FromImportStatement node)
        {
            if (node == _startAfter)
            {
                _startStatementEncountered = true;
            }
        }

        public override void PostWalk(FunctionDefinition node)
        {
            if (node == _startAfter)
            {
                _startStatementEncountered = true;
            }
        }

        public override void PostWalk(GlobalStatement node)
        {
            if (node == _startAfter)
            {
                _startStatementEncountered = true;
            }
        }

        public override bool Walk(IfStatement node)
        {
            if (_startStatementEncountered)
            {
                foreach (var test in node.Tests)
                {
                    EnterScope();
                    test.Walk(this);
                    ExitScope();
                }

                if (node.ElseStatement != null)
                {
                    EnterScope();
                    node.ElseStatement.Walk(this);
                    ExitScope();
                }

                return false;
            }

            var anyContainsStart = false;

            foreach (var test in node.Tests)
            {
                test.Walk(this);
                if (_startStatementEncountered)
                {
                    // This sub-node contains the start statement
                    anyContainsStart = true;
                    _startStatementEncountered = false; // Only consider it after the entire if has been processed
                }
            }

            node.ElseStatement?.Walk(this);

            if (anyContainsStart)
            {
                _startStatementEncountered = true;
            }

            return false;
        }

        public override void PostWalk(IfStatement node)
        {
            if (node == _startAfter)
            {
                _startStatementEncountered = true;
            }
        }

        public override void PostWalk(ImportStatement node)
        {
            if (node == _startAfter)
            {
                _startStatementEncountered = true;
            }
        }

        public override void PostWalk(PrintStatement node)
        {
            if (node == _startAfter)
            {
                _startStatementEncountered = true;
            }
        }

        public override void PostWalk(PythonAst node)
        {
            if (node == _startAfter)
            {
                _startStatementEncountered = true;
            }
        }

        public override void PostWalk(RaiseStatement node)
        {
            if (node == _startAfter)
            {
                _startStatementEncountered = true;
            }
        }

        public override void PostWalk(ReturnStatement node)
        {
            if (node == _startAfter)
            {
                _startStatementEncountered = true;
            }
        }

        public override void PostWalk(SuiteStatement node)
        {
            if (node == _startAfter)
            {
                _startStatementEncountered = true;
            }
        }

        public override void PostWalk(TryStatement node)
        {
            if (node == _startAfter)
            {
                _startStatementEncountered = true;
            }
        }

        public override void PostWalk(WhileStatement node)
        {
            if (node == _startAfter)
            {
                _startStatementEncountered = true;
            }
        }

        public override void PostWalk(WithStatement node)
        {
            if (node == _startAfter)
            {
                _startStatementEncountered = true;
            }
        }

        public override void PostWalk(NameExpression node)
        {
            if (_startStatementEncountered && !_definedInThisScope.Contains(node.Name))
            {
                ReferencedNames.Add(node.Name);
            }
        }
    }
}