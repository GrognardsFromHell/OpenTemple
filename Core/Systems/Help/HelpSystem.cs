using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.TabFiles;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.RollHistory;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Systems.Help;

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

    public List<string> VirtualChildren = new();

    public string Title;

    public string Text { get; set; }

    public readonly List<D20HelpLink> Links = new();

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
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

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

    // Used for topics that have no relationship to other topics and will not be regarded as siblings
    private const string DummyTag = "TAG_DUMMY";

    private readonly Dictionary<string, D20HelpTopic> _helpTopics = new();

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
    private void ParseHelpTopicIndex(in TabFileRecord record)
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
        }

        var tok = new Tokenizer(record[ColumnnVirtualParents].AsString());
        var vParents = new List<string>();
        while (tok.NextToken())
        {
            if (tok.IsIdentifier)
            {
                vParents.Add(tok.TokenText.ToString().ToUpperInvariant());
            }
        }

        topic.VirtualParents = vParents;

        topic.Title = record[ColumnTitle].AsString();
        _helpTopics[topic.Id] = topic;
    }

    private static readonly byte[] ChildrenPlaceholder = Encoding.Default.GetBytes("[CMD_CHILDREN]");
    private static readonly byte[] ChildrenSortedPlaceholder = Encoding.Default.GetBytes("[CMD_CHILDREN_SORTED]");

    // Appends the text accumulated so far to then skip a certain number of bytes
    private static void SkipBytes(ReadOnlySpan<byte> text, int bytesToSkip, ref int startOfSegment, ref int i,
        StringBuilder resultText)
    {
        var segment = text.Slice(startOfSegment, i - startOfSegment);
        DecodeAndAppend(segment, resultText);
        startOfSegment = i + bytesToSkip;
        i += bytesToSkip;
    }

    [TempleDllLocation(0x100e7cf0)]
    private void ParseHelpTopicText(in TabFileRecord record)
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
            // if (LinkParser.ParseLink(remainingText, out var linkText, out var linkTarget, out var linkLength))
            // {
            //     SkipBytes(text, linkLength, ref startOfSegment, ref i, resultText);
            //     topic.Links.Add(BuildLink(id, linkText, linkTarget, resultText));
            // }
            /*else*/ if (remainingText.StartsWith(ChildrenPlaceholder))
            {
                SkipBytes(text, ChildrenPlaceholder.Length, ref startOfSegment, ref i, resultText);
                AppendChildren(topic, resultText, false);
            }
            else if (remainingText.StartsWith(ChildrenSortedPlaceholder))
            {
                SkipBytes(text, ChildrenSortedPlaceholder.Length, ref startOfSegment, ref i, resultText);
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
            resultText.AppendFormat("~{0}~[{1}]\n", child.Title, child.Id);
        }
    }

    [TempleDllLocation(0x100e7060)]
    public D20HelpTopic RootTopic => _helpTopics[hashTAG_ROOT];

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

    public bool TryParseLink(string topicId, ReadOnlySpan<char> linkTarget, out D20HelpLink helpLink)
    {
        if (linkTarget.StartsWith("TAG_"))
        {
            helpLink = CreateTopicLink(topicId, new string(linkTarget), "", new StringBuilder());
            return true;
        }
        else if (linkTarget.StartsWith("ROLL_"))
        {
            var rollId = int.Parse(linkTarget.Slice("ROLL_".Length));
            helpLink = CreateRollLink(rollId, "", new StringBuilder());
            return true;
        }
        else
        {
            helpLink = default;
            return false;
        }
    }

    [TempleDllLocation(0x100e7670)]
    public bool TryParseLink(string topicId, ReadOnlySpan<char> text,
        StringBuilder resultText, out int charsConsumed, out D20HelpLink helpLink)
    {
        if (!UnicodeLinkParser.ParseLink(text, out var linkText, out var linkTarget, out charsConsumed))
        {
            helpLink = default;
            charsConsumed = 0;
            return false;
        }

        if (linkTarget.StartsWith("TAG_"))
        {
            helpLink = CreateTopicLink(topicId, new string(linkTarget), linkText, resultText);
            return true;
        }
        else if (linkTarget.StartsWith("ROLL_"))
        {
            var rollId = int.Parse(linkTarget.Slice("ROLL_".Length));
            helpLink = CreateRollLink(rollId, linkText, resultText);
            return true;
        }
        else
        {
            helpLink = default;
            return false;
        }
    }

    [TempleDllLocation(0x100e74b0)]
    private D20HelpLink CreateTopicLink(string topicId, string linkTarget,
        ReadOnlySpan<byte> linkText, StringBuilder resultText)
    {
        Span<char> decoded = stackalloc char[TextEncoding.GetMaxCharCount(linkText.Length)];
        var actualChars = TextEncoding.GetChars(linkText, decoded);
        var linkTextChars = decoded.Slice(0, actualChars);

        return CreateTopicLink(topicId, linkTarget, linkTextChars, resultText);
    }

    private D20HelpLink CreateTopicLink(string topicId, string linkTarget,
        ReadOnlySpan<char> linkText, StringBuilder resultText)
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

        resultText.Append(linkText);
        resultText.Append("@0");
        link.Length = resultText.Length - link.StartPos;
        return link;
    }

    private D20HelpLink CreateRollLink(int rollId, ReadOnlySpan<byte> linkText, StringBuilder resultText)
    {
        Span<char> decoded = stackalloc char[TextEncoding.GetMaxCharCount(linkText.Length)];
        var actualChars = TextEncoding.GetChars(linkText, decoded);
        var linkTextChars = decoded.Slice(0, actualChars);

        return CreateRollLink(rollId, linkTextChars, resultText);
    }

    [TempleDllLocation(0x100e75e0)]
    private D20HelpLink CreateRollLink(int rollId, ReadOnlySpan<char> linkText, StringBuilder resultText)
    {
        var link = new D20HelpLink();
        link.IsRoll = true;
        link.RollId = rollId;
        link.StartPos = resultText.Length;

        resultText.Append("@1");
        resultText.Append(linkText);
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
        if (topic.PrevId != null && topic.PrevId != hashTAG_ROOT)
        {
            if (_helpTopics.TryGetValue(topic.PrevId, out var prevSibling))
            {
                // Put it into the same parent topic as the previous node and link it up
                topic.ParentId = prevSibling.ParentId;
                prevSibling.NextId = topic.Id;
                return;
            }
            
            Logger.Warn("Help topic '{0}' has unknown previous-sibling id '{1}'.", topic, topic.PrevId);
        }
        
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
    private int help_table_pad = -1; // I believe this may be "last topic requested"

    [TempleDllLocation(0x11867700)]
    private readonly List<HelpRequest> _helpRequests = new ();

    private HelpRequest CurrentRequest
    {
        get
        {
            if (help_table_pad >= 0 && help_table_pad < _helpRequests.Count)
            {
                return _helpRequests[help_table_pad];
            }

            return null;
        }
    }

    [TempleDllLocation(0x100e6db0)]
    public bool CanNavigateBackward => CurrentRequest != null && help_table_pad > 0;

    [TempleDllLocation(0x100e6f40)]
    public void NavigateBackward()
    {
        if (help_table_pad > 0)
        {
            help_table_pad--;
            GameUiBridge.ShowHelp(CurrentRequest, false);
        }
    }

    [TempleDllLocation(0x100e6dc0)]
    public bool CanNavigateForward => CurrentRequest != null && help_table_pad < _helpRequests.Count - 1;

    [TempleDllLocation(0x100e6f70)]
    public void NavigateForward()
    {
        if (help_table_pad + 1 < _helpRequests.Count)
        {
            help_table_pad++;
            GameUiBridge.ShowHelp(CurrentRequest, false);
        }
    }

    [TempleDllLocation(0x100e6de0)]
    public bool CanNavigateUp
    {
        get
        {
            var currentRequest = CurrentRequest;
            if (currentRequest == null || currentRequest.Type != HelpRequestType.HelpTopic)
            {
                return false;
            }

            return currentRequest.Topic.ParentId != DummyTag && currentRequest.Topic.Id != hashTAG_ROOT;
        }
    }

    [TempleDllLocation(0x100e6fd0)]
    public void NavigateUp()
    {
        if ( CanNavigateUp )
        {
            GameSystems.Help.ShowTopic(CurrentRequest.Topic.ParentId);
        }
    }

    [TempleDllLocation(0x100e6e40)]
    public bool CanNavigateToPreviousSibling
    {
        get
        {
            var currentRequest = CurrentRequest;
            if (currentRequest == null || currentRequest.Type != HelpRequestType.HelpTopic)
            {
                return false;
            }

            var topic = currentRequest.Topic;
            if (topic.ParentId == DummyTag)
            {
                // Topics within the dummy tag have no relationship with each other
                return false;
            }

            return topic.PrevId != null;
        }
    }

    [TempleDllLocation(0x100e6fd0)]
    public void NavigateToPreviousSibling()
    {
        if ( CanNavigateToPreviousSibling )
        {
            GameSystems.Help.ShowTopic(CurrentRequest.Topic.PrevId);
        }
    }

    [TempleDllLocation(0x100e6eb0)]
    public bool CanNavigateToNextSibling
    {
        get
        {
            var currentRequest = CurrentRequest;
            if (currentRequest == null || currentRequest.Type != HelpRequestType.HelpTopic)
            {
                return false;
            }

            var topic = currentRequest.Topic;
            if (topic.ParentId == DummyTag)
            {
                // Topics within the dummy tag have no relationship with each other
                return false;
            }

            return topic.NextId != null;
        }
    }

    [TempleDllLocation(0x100e6fd0)]
    public void NavigateToNextSibling()
    {
        if ( CanNavigateToNextSibling )
        {
            GameSystems.Help.ShowTopic(CurrentRequest.Topic.NextId);
        }
    }

    [TempleDllLocation(0x100e6c70)]
    private void ShowHelpRequest(HelpRequest request)
    {
        if (help_table_pad == -1 || !_helpRequests[help_table_pad].Equals(request))
        {
            _helpRequests.Add(request);
            help_table_pad = _helpRequests.Count - 1;
        }
        GameUiBridge.ShowHelp(_helpRequests[help_table_pad], false);
    }

    [TempleDllLocation(0x100e6d50)]
    public void ShowRoll(int historyId)
    {
        var historyEntry = GameSystems.RollHistory.FindEntry(historyId);
        if (historyEntry != null)
        {
            ShowHelpRequest(new HelpRequest(historyEntry));
        }
    }

    public void OpenLink(D20HelpLink link)
    {
        if (link.IsRoll)
        {
            var historyEntry = GameSystems.RollHistory.FindEntry(link.RollId);
            if (historyEntry != null)
            {
                ShowHelpRequest(new HelpRequest(historyEntry));
            }
            else
            {
                Logger.Warn("Tried to show roll history with id {0}, but it doesn't exist!", link.RollId);
            }
        }
        else
        {
            ShowHelpRequest(new HelpRequest(link.LinkedTopic));
        }
    }

    [TempleDllLocation(0x100e6cf0)]
    public void ShowTopic(string topicId)
    {
        if (_helpTopics.TryGetValue(topicId, out var topic))
        {
            ShowHelpRequest(new HelpRequest(topic));
        }
        else
        {
            Logger.Warn("Trying to request help for unknown help topic '{0}'", topicId);
        }
    }

    [TempleDllLocation(0x100e6f10)]
    public void ShowAlert(string topicId, Action<int> callback, string buttonText)
    {
        var request = new HelpRequest(_helpTopics[topicId]);
        GameUiBridge.ShowAlert(request, callback, buttonText);
    }

    [TempleDllLocation(0x100e7030)]
    public bool TryGetTopic(string topicId, out D20HelpTopic topic)
    {
        if (_helpTopics.TryGetValue(topicId, out topic))
        {
            return true;
        }

        return false;
    }
}

public enum HelpRequestType
{
    HelpTopic = 0,
    RollHistoryEntry = 1,
    Custom = 2
}

public class HelpRequest
{
    public HelpRequestType Type { get; }
    public D20HelpTopic Topic { get; }
    public HistoryEntry RollHistoryEntry { get; }
    public string CustomHeader { get; }
    public string CustomBody { get; }

    public HelpRequest(D20HelpTopic topic)
    {
        Type = HelpRequestType.HelpTopic;
        Topic = topic;
    }

    public HelpRequest(HistoryEntry historyEntry)
    {
        Type = HelpRequestType.RollHistoryEntry;
        RollHistoryEntry = historyEntry;
    }

    public HelpRequest(string customHeader, string customBody)
    {
        Type = HelpRequestType.Custom;
        CustomHeader = customHeader;
        CustomBody = customBody;
    }

    protected bool Equals(HelpRequest other)
    {
        return Type == other.Type && Equals(Topic, other.Topic) && Equals(RollHistoryEntry, other.RollHistoryEntry);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((HelpRequest) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (int) Type;
            hashCode = (hashCode * 397) ^ (Topic != null ? Topic.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (RollHistoryEntry != null ? RollHistoryEntry.GetHashCode() : 0);
            return hashCode;
        }
    }

    public static bool operator ==(HelpRequest left, HelpRequest right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(HelpRequest left, HelpRequest right)
    {
        return !Equals(left, right);
    }
}