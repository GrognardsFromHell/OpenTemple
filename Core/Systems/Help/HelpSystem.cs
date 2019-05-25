using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.IO.TabFiles;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems.Help
{
    public enum D20HelpType
    {
        Default = 0,
        Alignments,
        Classes,
        Feats,
        Races,
        Skills,
        Spells,
        UI
    }

    public class D20HelpTopic
    {
        public string Id;
        public string ParentId;
        public string NextId;
        public string PrevId;
        public string SiblingId;

        // ids for topics which will list this topic when using the command [CMD_CHILDREN] inside the text body
        public List<string> VirtualParents;

        public List<string> VirtualChildren = new List<string>();

        public string Title;

        public string Text { get; set; }

        public readonly List<D20HelpLink> Links = new List<D20HelpLink>();

        public override string ToString()
        {
            return Id;
        }
    }

    public struct D20HelpLink
    {
        public bool IsRoll; // is 1 for ROLL_ type links, 0 otherwise
        public D20HelpTopic LinkedTopic;
        public int RollId;
        public int StartPos;
        public int Length;
    }

    public class HelpSystem : IGameSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        [TempleDllLocation(0x10BD01B8)]
        public const bool IsEditor = false;

        private const int HELP_IDX_UI = 1; // 1 to 74;   75,76 are separators
        private const int HELP_IDX_ALIGNMENT = 76; // 76 to 86;    87, 88 are separators
        private const int HELP_IDX_CLASSES = 88; // 89 to 99;    100,102 are separators
        private const int HELP_IDX_RACES = 101; // 101 to 108;   108, 109 are separators
        private const int HELP_IDX_FEATS = 109; // 109 to 757
        private const int HELP_IDX_SKILLS = 759; // 759 to 858

        // vanilla spell are mapped to 860 up to 860 + 802 (the vanilla spell count)
        private const int HELP_IDX_SPELLS = 860;

        private const int HELP_IDX_VANILLA_MAX = 1661;

        [TempleDllLocation(0x10BD01BC)]
        private const string hashTAG_ROOT = "TAG_ROOT";

        private readonly Dictionary<string, D20HelpTopic> _helpTopics = new Dictionary<string, D20HelpTopic>();

        private const string TemplePlusExtensionsFile = "tpmes/help_extensions.tab";

        [TempleDllLocation(0x100e6c40)]
        public HelpSystem()
        {
            Logger.Info("Parsing Help Data...");

            // Open the files and do preliminary parsing
            var helpFiles = GetHelpFiles();

            foreach (var helpFile in helpFiles)
            {
                TabFile.ParseFile(helpFile, ParseHelpTopicIndex);
            }

            foreach (var helpTopic in _helpTopics.Values)
            {
                LinkTopics(helpTopic);
            }

            foreach (var helpFile in helpFiles)
            {
                TabFile.ParseFile(helpFile, ParseHelpTopicText);
            }
        }

        private List<string> GetHelpFiles()
        {
            var result = new List<string>();
            result.Add("mes/help.tab");
            if (Tig.FS.FileExists(TemplePlusExtensionsFile))
            {
                TabFile.ParseFile(TemplePlusExtensionsFile, ParseHelpTopicIndex);
            }

            foreach (var filename in Tig.FS.Search("mes/help/*.tab"))
            {
                result.Add(filename);
            }

            return result;
        }

        private const int ColumnId = 0;
        private const int ColumnParentId = 1;
        private const int ColumnPrevId = 2;
        private const int ColumnnVirtualParents = 3;
        private const int ColumnTitle = 4;
        private const int ColumnText = 5;

        [TempleDllLocation(0x100e7120)]
        private void ParseHelpTopicIndex(TabFileRecord record)
        {
            var topic = new D20HelpTopic();

            topic.Id = record[ColumnId].AsString().ToUpperInvariant();

            if (record[ColumnParentId])
            {
                topic.ParentId = record[ColumnParentId].AsString().ToUpperInvariant();
            }

            if (record[ColumnPrevId])
            {
                topic.PrevId = record[ColumnPrevId].AsString().ToUpperInvariant();
                Stub.TODO();
                // We are not handling the prevId correctly as it was handled before (see LinkTopics)
            }

            var tok = new Tokenizer(record[ColumnnVirtualParents].AsString());
            var vParents = new List<string>();
            while (tok.NextToken())
            {
                if (tok.IsIdentifier)
                {
                    vParents.Add(tok.TokenText.ToUpperInvariant());
                }
            }

            topic.VirtualParents = vParents;

            topic.Title = record[ColumnTitle].AsString();
            _helpTopics[topic.Id] = topic;
        }

        private static readonly byte[] ChildrenPlaceholder = Encoding.Default.GetBytes("[CMD_CHILDREN]");
        private static readonly byte[] ChildrenSortedPlaceholder = Encoding.Default.GetBytes("[CMD_CHILDREN_SORTED]");

        [TempleDllLocation(0x100e7cf0)]
        private void ParseHelpTopicText(TabFileRecord record)
        {
            var id = record[ColumnId].AsString();
            if (!_helpTopics.TryGetValue(id, out var topic))
            {
                return;
            }

            ReadOnlySpan<byte> text = record[ColumnText];
            var resultText = new StringBuilder(4096);
            var startOfSegment = 0;

            var i = 0;
            while (i < text.Length)
            {
                var remainingText = text.Slice(i);
                if (LinkParser.ParseLink(remainingText, out var linkText, out var linkTarget, out var linkLength))
                {
                    var segment = text.Slice(startOfSegment, i - startOfSegment);
                    DecodeAndAppend(segment, resultText);
                    startOfSegment = i + linkLength;
                    i += linkLength;

                    topic.Links.Add(BuildLink(id, linkText, linkTarget, resultText));
                }
                else if (remainingText.StartsWith(ChildrenPlaceholder))
                {
                    i += ChildrenPlaceholder.Length;
                    AppendChildren(topic, resultText, false);
                }
                else if (remainingText.StartsWith(ChildrenSortedPlaceholder))
                {
                    i += ChildrenSortedPlaceholder.Length;
                    AppendChildren(topic, resultText, true);
                }
                else
                {
                    i++;
                }
            }

            var lastSegment = text.Slice(startOfSegment, text.Length - startOfSegment);
            DecodeAndAppend(lastSegment, resultText);

            // Replace vertical tabs with newlines
            resultText.Replace('\v', '\n');

            topic.Text = resultText.ToString();
        }

        [TempleDllLocation(0x100e7930)]
        private void AppendChildren(D20HelpTopic topic, StringBuilder resultText, bool sorted)
        {
            var children = new List<D20HelpTopic>();
            children.AddRange(EnumerateDirectChildren(topic));
            children.AddRange(topic.VirtualChildren.Select(id => _helpTopics[id]));

            if (sorted)
            {
                children.Sort((a, b) => string.Compare(a.Title, b.Title, StringComparison.OrdinalIgnoreCase));
            }

            if (children.Count == 0)
            {
                Logger.Warn("Topic '{0}' has children tag, but no children.", topic.Id);
            }

            foreach (var child in children)
            {
                BuildLink(
                    topic.Id,
                    Encoding.Default.GetBytes(child.Title),
                    Encoding.Default.GetBytes(child.Id),
                    resultText
                );
            }
        }

        private IEnumerable<D20HelpTopic> EnumerateDirectChildren(D20HelpTopic topic)
        {
            var siblingId = topic.SiblingId;
            while (siblingId != null)
            {
                if (_helpTopics.TryGetValue(siblingId, out var sibling))
                {
                    yield return sibling;
                    siblingId = sibling.NextId;
                }
                else
                {
                    break;
                }
            }
        }

        [TempleDllLocation(0x100e7670)]
        private D20HelpLink BuildLink(string topicId, ReadOnlySpan<byte> linkText, ReadOnlySpan<byte> linkTarget,
            StringBuilder resultText)
        {
            var linkTargetStr = Encoding.Default.GetString(linkTarget);
            if (linkTargetStr.StartsWith("TAG_"))
            {
                return CreateTopicLink(topicId, linkTargetStr, linkText, resultText);
            }
            else if (linkTargetStr.StartsWith("ROLL_"))
            {
                var rollId = int.Parse(linkTargetStr.Substring("ROLL_".Length));
                return CreateRollLink(rollId, linkTarget, resultText);
            }
            else
            {
                Logger.Warn("Unknown link target '{0}' in topic '{1}'", linkTargetStr, topicId);
                return default;
            }
        }

        [TempleDllLocation(0x100e74b0)]
        private D20HelpLink CreateTopicLink(string topicId, string linkTarget,
            ReadOnlySpan<byte> linkText, StringBuilder resultText)
        {
            var link = new D20HelpLink();
            link.IsRoll = false;
            link.StartPos = resultText.Length;
            if (_helpTopics.TryGetValue(linkTarget, out link.LinkedTopic))
            {
                resultText.Append("@1");
            }
            else
            {
                resultText.Append("@2");
                Logger.Warn("Topic '{0}' has broken link to '{1}'", topicId, linkTarget);
            }

            DecodeAndAppend(linkText, resultText);
            resultText.Append("@0");
            link.Length = resultText.Length - link.StartPos;
            return link;
        }

        [TempleDllLocation(0x100e75e0)]
        private D20HelpLink CreateRollLink(int rollId, ReadOnlySpan<byte> linkText, StringBuilder resultText)
        {
            var link = new D20HelpLink();
            link.IsRoll = true;
            link.RollId = rollId;
            link.StartPos = resultText.Length;

            resultText.Append("@1");
            DecodeAndAppend(linkText, resultText);
            resultText.Append("@0");

            link.Length = resultText.Length - link.StartPos;

            return link;
        }

        private static readonly Encoding TextEncoding = Encoding.Default;

        private static void DecodeAndAppend(ReadOnlySpan<byte> encoded, StringBuilder result)
        {
            Span<char> decoded = stackalloc char[TextEncoding.GetMaxCharCount(encoded.Length)];
            var actualChars = TextEncoding.GetChars(encoded, decoded);
            result.Append(decoded.Slice(0, actualChars));
        }

        [TempleDllLocation(0x100e7280)]
        private void LinkTopics(D20HelpTopic topic)
        {
            if (topic.Id == hashTAG_ROOT)
            {
                topic.ParentId = null;
                return;
            }

            D20HelpTopic parentNode;
            if (topic.ParentId == null)
            {
                topic.ParentId = hashTAG_ROOT;
                parentNode = _helpTopics[hashTAG_ROOT];
            }
            else
            {
                if (!_helpTopics.TryGetValue(topic.ParentId, out parentNode))
                {
                    Logger.Warn("Help topic '{0}' has unknown parent id '{1}'. Attaching to root.", topic,
                        topic.ParentId);
                    topic.ParentId = hashTAG_ROOT;
                    parentNode = _helpTopics[hashTAG_ROOT];
                }
            }

            // Sibling ID just appends the topic to a chain of topics starting with the given id
            if (parentNode.SiblingId != null)
            {
                var prev = parentNode.SiblingId;
                D20HelpTopic sibling;
                do
                {
                    if (!_helpTopics.TryGetValue(prev, out sibling))
                    {
                        Logger.Warn("Topic '{0}' has an unknown sibling '{1}'.", parentNode, prev);
                        break;
                    }

                    prev = sibling.NextId;
                } while (sibling.NextId != null);

                if (sibling != null)
                {
                    sibling.NextId = topic.Id;
                    topic.PrevId = sibling.Id;
                }
            }
            else
            {
                parentNode.SiblingId = topic.Id;
            }

            foreach (var parentTopicId in topic.VirtualParents)
            {
                if (!_helpTopics.TryGetValue(parentTopicId, out var parentTopic))
                {
                    Logger.Warn("Topic '{0}' has unknown parent '{1}'.", topic, parentTopicId);
                    continue;
                }

                parentTopic.VirtualChildren.Add(topic.Id);
            }
        }

        public void Dispose()
        {
        }

        [TempleDllLocation(0x118676E0)]
        private int help_table_pad = -1;

        private List<HelpRequest> _helpRequests = new List<HelpRequest>();

        [TempleDllLocation(0x100e6cf0)]
        public void ShowTopic(string topicId)
        {
            if (help_table_pad == -1 || _helpRequests[help_table_pad].state != 0
                                     || _helpRequests[help_table_pad].topicId != topicId)
            {
                var request = new HelpRequest();
                request.topicId = topicId;
                _helpRequests.Add(request);
                help_table_pad = _helpRequests.Count - 1;
            }
            else
            {
                GameUiBridge.ShowHelp(_helpRequests[help_table_pad], 0);
            }
        }

    }

    public class HelpRequest
    {
        public int state;
        public string topicId;
        public string text;
    }

}