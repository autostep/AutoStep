using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using AutoStep.Definitions;
using AutoStep.Elements.Parts;
using AutoStep.Elements.StepTokens;

namespace AutoStep.Language.Test.Matching
{
    /// <summary>
    /// Represents a single node in the matching tree used to locate step definitions.
    /// </summary>
    internal class MatchingTreeNode
    {
        /// <summary>
        /// Gets the set of all definitions.
        /// </summary>
        private readonly LinkedList<StepDefinition> allDefinitions;

        /// <summary>
        /// Gets the part used for matching this node.
        /// </summary>
        private readonly DefinitionPart? matchingPart;

        /// <summary>
        /// The left-most (first) step definition from this node downwards.
        /// </summary>
        private LinkedListNode<StepDefinition>? leftDefinition;

        /// <summary>
        /// The right-most (last) step definition from this node downwards.
        /// </summary>
        private LinkedListNode<StepDefinition>? rightDefinition;

        /// <summary>
        /// The set of definitions that are an exact match for this point in the tree.
        /// </summary>
        private LinkedList<LinkedListNode<StepDefinition>>? exactMatchNodes;

        /// <summary>
        /// The children of this node.
        /// </summary>
        private LinkedList<MatchingTreeNode>? children;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchingTreeNode"/> class.
        /// </summary>
        /// <param name="allStepDefinitions">The set of all definitions.</param>
        /// <param name="matchingPart">The matching part used by this node.</param>
        public MatchingTreeNode(LinkedList<StepDefinition> allStepDefinitions, DefinitionPart? matchingPart)
        {
            allDefinitions = allStepDefinitions;
            this.matchingPart = matchingPart;
        }

        private bool IsEmpty => leftDefinition == null;

        /// <summary>
        /// Add a definition to this node.
        /// </summary>
        /// <param name="definitionNode">The definition node.</param>
        /// <param name="allDefinitionParts">The set of all matching parts associated to the definition.</param>
        /// <param name="nextPartPosition">The position in allDefinitionParts that this node should look at for comparison.</param>
        public void AddDefinition(
            LinkedListNode<StepDefinition> definitionNode,
            IReadOnlyList<DefinitionPart> allDefinitionParts,
            int nextPartPosition)
        {
            var addPosition = nextPartPosition;
            nextPartPosition++;

            if (leftDefinition is null)
            {
                // Determine the left position (starts out as the first def for the node).
                leftDefinition = definitionNode;
            }

            // Is there a part to look at?
            if (addPosition < allDefinitionParts.Count)
            {
                // Yes, so we need to check if any of our children are a match.
                var newPart = allDefinitionParts[addPosition];

                // We are not at the end of the list, that means this node needs children.
                if (children is null)
                {
                    // No, we need to create one.
                    children = new LinkedList<MatchingTreeNode>();
                }

                // Search for an equivalent match.
                var nextSearch = children.First;

                while (nextSearch is object)
                {
                    var nodeValue = nextSearch.Value;

                    if (nodeValue.matchingPart!.IsDefinitionPartMatch(newPart))
                    {
                        // Found an exact match for the new part, this means that one of the children is a match.
                        nodeValue.AddDefinition(definitionNode, allDefinitionParts, nextPartPosition);
                        break;
                    }

                    nextSearch = nextSearch.Next;
                }

                if (nextSearch is null)
                {
                    // Out of things to search, looks like a new child.
                    var child = new MatchingTreeNode(allDefinitions, newPart);

                    children.AddLast(child);

                    child.AddDefinition(definitionNode, allDefinitionParts, nextPartPosition);
                }
            }
            else
            {
                if (exactMatchNodes == null)
                {
                    exactMatchNodes = new LinkedList<LinkedListNode<StepDefinition>>();
                }

                // I need to add the definition node to the linked list (it is now the furthest right-hand side of the list).
                if (rightDefinition is null)
                {
                    allDefinitions.AddLast(definitionNode);

                    // This is the last position in the list, so it must be an exact match.
                    exactMatchNodes.AddLast(definitionNode);
                }
                else
                {
                    // Check if this definition already exists (and should just be replaced).
                    LinkedListNode<StepDefinition>? currentNode = leftDefinition;
                    while (currentNode != null)
                    {
                        // This is the same definition in the same source (but possibly a new version).
                        // Replace it.
                        if (DefinitionsAreTheSame(currentNode.Value, definitionNode.Value))
                        {
                            // Just replace the node in-place.
                            currentNode.Value = definitionNode.Value;
                            break;
                        }

                        if (currentNode == rightDefinition)
                        {
                            currentNode = null;
                        }
                        else
                        {
                            currentNode = currentNode.Next;
                        }
                    }

                    // No existing node found, just add the new definition.
                    if (currentNode is null)
                    {
                        allDefinitions.AddAfter(rightDefinition, definitionNode);
                        exactMatchNodes.AddLast(definitionNode);
                    }
                }
            }

            rightDefinition = definitionNode;
        }

        private static bool DefinitionsAreTheSame(StepDefinition left, StepDefinition right)
        {
            return left.Source == right.Source && left.IsSameDefinition(right);
        }

        /// <summary>
        /// Remove a definition from the node.
        /// </summary>
        /// <param name="definition">The definition being removed.</param>
        /// <param name="allDefinitionParts">The step matching parts.</param>
        /// <param name="nextPartPosition">The next position in the matching parts.</param>
        /// <returns>The linked list node containing the located step definition (if it's found in this node or one of its children).</returns>
        public LinkedListNode<StepDefinition>? RemoveDefinition(StepDefinition definition, IReadOnlyList<DefinitionPart> allDefinitionParts, int nextPartPosition)
        {
            var removePosition = nextPartPosition;
            nextPartPosition++;
            LinkedListNode<StepDefinition>? removingNode = null;

            // Any parts left?
            if (removePosition < allDefinitionParts.Count)
            {
                // Yes, so we need to check if any of our children are a match.
                var newPart = allDefinitionParts[removePosition];

                if (children is object)
                {
                    // Search for an equivalent match.
                    var nextSearch = children.First;

                    while (nextSearch is object)
                    {
                        var nodeValue = nextSearch.Value;

                        if (nodeValue.matchingPart!.IsDefinitionPartMatch(newPart))
                        {
                            // Found an exact match for the new part, this means that one of the children is a match.
                            removingNode = nodeValue.RemoveDefinition(definition, allDefinitionParts, nextPartPosition);

                            // This path has removed the definition.
                            if (removingNode is object && nodeValue.IsEmpty)
                            {
                                children.Remove(nextSearch);
                            }

                            break;
                        }

                        nextSearch = nextSearch.Next;
                    }
                }
            }
            else
            {
                // Find the actual definition node.
                var foundNode = leftDefinition;

                while (foundNode != null)
                {
                    // This is the same definition (but possibly a new version).
                    // Replace it.
                    if (DefinitionsAreTheSame(foundNode.Value, definition))
                    {
                        if (exactMatchNodes is object)
                        {
                            exactMatchNodes.Remove(foundNode);
                        }

                        removingNode = foundNode;

                        break;
                    }

                    if (foundNode == rightDefinition)
                    {
                        foundNode = null;
                    }
                    else
                    {
                        foundNode = foundNode.Next;
                    }
                }
            }

            if (removingNode is object)
            {
                // Found it - remove it.
                if (leftDefinition == removingNode)
                {
                    // Single definition item - now basically doesn't contain anything.
                    if (leftDefinition == rightDefinition)
                    {
                        // Equivalent of stating that the node is empty.
                        leftDefinition = rightDefinition = null;
                    }
                    else
                    {
                        // Move the left position along.
                        leftDefinition = leftDefinition.Next;
                    }
                }
                else if (rightDefinition == removingNode)
                {
                    // Bring the right-hand-side in.
                    rightDefinition = removingNode.Previous;
                }

                if (removePosition == 0)
                {
                    // We are back at the top, remove the definition.
                    allDefinitions.Remove(removingNode);
                }
            }

            return removingNode;
        }

        /// <summary>
        /// Starts searching for matches starting from this node, treating it as a root node.
        /// </summary>
        /// <param name="results">A set to add match results to.</param>
        /// <param name="referenceText">The step reference text to search through.</param>
        /// <param name="allSearchTokens">The set of search tokens to go through.</param>
        /// <param name="exactOnly">Whether to return exact matches only.</param>
        /// <param name="partsMatched">Sets a value containing the number of parts (out of allSearchParts) that were matched during the search.</param>
        public void SearchRoot(LinkedList<MatchResult> results, string referenceText, ReadOnlySpan<StepToken> allSearchTokens, bool exactOnly, ref int partsMatched)
        {
            if (children is object)
            {
                var currentChild = children.First;
                while (currentChild is object)
                {
                    currentChild.Value.SearchMatches(results, referenceText, allSearchTokens, exactOnly, default, out var finalSpan);

                    var handledSize = allSearchTokens.Length - finalSpan.Length;

                    if (handledSize > partsMatched)
                    {
                        partsMatched = handledSize;
                    }

                    currentChild = currentChild.Next;
                }
            }
        }

        /// <summary>
        /// Searches this node (and all children, if needed) for matches against the node's part.
        /// </summary>
        /// <param name="results">A set to add match results to.</param>
        /// <param name="referenceText">The text to search against.</param>
        /// <param name="remainingTokenSpan">The remaining set of tokens to search through.</param>
        /// <param name="exactOnly">Whether to return exact matches only.</param>
        /// <param name="placeHolderDictionary">Dictionary of all placeholder matches.</param>
        /// <param name="finalSpan">Outputs the span of tokens remaining after the search has completed down this path.</param>
        /// <returns>true if this node (or any child) added results to the list.</returns>
        private bool SearchMatches(LinkedList<MatchResult> results, string referenceText, ReadOnlySpan<StepToken> remainingTokenSpan, bool exactOnly, PlaceHolderCopyOnWriteDictionary placeHolderDictionary, out ReadOnlySpan<StepToken> finalSpan)
        {
            Debug.Assert(matchingPart is object);

            // Check for match quality between the part assigned to this node and the part we are looking for.
            var match = matchingPart.DoStepReferenceMatch(referenceText, remainingTokenSpan);

            finalSpan = match.RemainingTokens;

            if (match.Length == 0)
            {
                // No match, nothing at this node or below.
                return false;
            }

            var addAllRemaining = true;
            var ignoreExact = false;
            var addedSomething = false;

            if (matchingPart is PlaceholderMatchPart placeholderPart)
            {
                var placeholderValue = GetContent(referenceText, match.MatchedTokens);

                // A placeholder value needs to be the same for all parts in order to match.
                // We need to take the matched placeholder and remember it for this search path.
                if (placeHolderDictionary.TryGetValue(placeholderPart.PlaceholderValueName, out var existingValue))
                {
                    // If the existing value does not have the same value, then we do not match, and we will jump out
                    // (return false).
                    if (!string.Equals(placeholderValue, existingValue, StringComparison.CurrentCulture))
                    {
                        // Different placeholder value; not okay.
                        // Do not match on this placeholder.
                        return false;
                    }
                }
                else
                {
                    placeHolderDictionary.Set(placeholderPart.PlaceholderValueName, placeholderValue);
                }
            }

            // The current match has consumed the entirety of the rest of the reference.
            if (match.RemainingTokens.IsEmpty)
            {
                // If this is an exact match, do we have an exact step def for it?
                if (match.IsExact && exactMatchNodes is object && exactMatchNodes.Count > 0)
                {
                    ignoreExact = true;

                    foreach (var exact in exactMatchNodes)
                    {
                        results.AddFirst(new MatchResult(true, int.MaxValue, exact.Value));
                    }

                    addedSomething = true;
                }
            }
            else if (children is object)
            {
                // There's no point looking at the children unless this node was an exact match. If it's not an exact match
                // then there's no point going further in the tree.
                if (match.IsExact)
                {
                    var currentChild = children.First;

                    while (currentChild is object)
                    {
                        var childMatched = currentChild.Value.SearchMatches(results, referenceText, match.RemainingTokens, exactOnly, placeHolderDictionary.Copy(), out var searchDepthSpan);

                        if (childMatched)
                        {
                            // A more specific match was found, so don't use any results from higher in the tree.
                            addAllRemaining = false;
                            addedSomething = true;
                            finalSpan = searchDepthSpan;
                        }

                        currentChild = currentChild.Next;
                    }
                }
            }

            if (addAllRemaining)
            {
                if (!exactOnly)
                {
                    // Add whatever items we have between left and right definitions.
                    var currentNode = leftDefinition;

                    // Add every node up until the right-hand side.
                    while (currentNode != null)
                    {
                        if (!ignoreExact || (exactMatchNodes is object && !exactMatchNodes.Contains(currentNode)))
                        {
                            addedSomething = true;
                            results.AddLast(new MatchResult(false, match.Length, currentNode.Value));
                        }

                        if (currentNode == rightDefinition)
                        {
                            currentNode = null;
                        }
                        else
                        {
                            currentNode = currentNode.Next;
                        }
                    }
                }
            }

            if (addedSomething && results.First.Value.IsExact)
            {
                if (matchingPart is ArgumentPart arg)
                {
                    var currentResult = results.First;

                    // Only worry about exact matches (and all the exacts come at the start of the list).
                    while (currentResult is object && currentResult.Value.IsExact)
                    {
                        // When at least one exact match has been added, it means that this node's
                        // argument binding resulted in a final match.
                        var exactMatch = currentResult.Value;

                        // Add to the list of 'argument token spans'.
                        exactMatch.PrependArgumentSet(arg, match);

                        currentResult = currentResult.Next;
                    }
                }
                else if (matchingPart is PlaceholderMatchPart placeholder &&
                         placeHolderDictionary.TryGetValue(placeholder.PlaceholderValueName, out var placeholderValue))
                {
                    var currentResult = results.First;

                    // Only worry about exact matches (and all the exacts come at the start of the list).
                    while (currentResult is object && currentResult.Value.IsExact)
                    {
                        // When at least one exact match has been added, it means that this node's
                        // placeholder binding resulted in a final match.
                        var exactMatch = currentResult.Value;

                        // Set the placeholder result in the final item.
                        exactMatch.IncludePlaceholderValue(placeholder.PlaceholderValueName, placeholderValue);

                        currentResult = currentResult.Next;
                    }
                }
            }

            return addedSomething;
        }

        /// <summary>
        /// Returns the content of the matched text, given the reference text.
        /// </summary>
        /// <param name="referenceText">The entire reference text.</param>
        /// <param name="matchedTokens">The set of matched tokens.</param>
        /// <returns>The placeholder content.</returns>
        private static string GetContent(string referenceText, ReadOnlySpan<StepToken> matchedTokens)
        {
            // Content of a placeholder will just be the literal text between the start and end.
            if (matchedTokens.Length == 0)
            {
                return string.Empty;
            }

            if (matchedTokens.Length == 1)
            {
                var matched = matchedTokens[0];
                return referenceText.Substring(matched.StartIndex, matched.Length);
            }
            var start = matchedTokens[0].StartIndex;
            var lastToken = matchedTokens[matchedTokens.Length - 1];
            var length = (lastToken.StartIndex - start) + lastToken.Length;

            return referenceText.Substring(start, length);
        }

        private struct PlaceHolderCopyOnWriteDictionary
        {
            private Dictionary<string, string>? originalDictionary;
            private Dictionary<string, string>? workingDictionary;

            public PlaceHolderCopyOnWriteDictionary Copy()
            {
                return new PlaceHolderCopyOnWriteDictionary
                {
                    originalDictionary = workingDictionary ?? originalDictionary,
                };
            }

            public bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
            {
                var dictionary = workingDictionary ?? originalDictionary;

                if (dictionary is null)
                {
                    value = null;
                    return false;
                }

                return dictionary.TryGetValue(key, out value);
            }

            public void Set(string key, string value)
            {
                if (workingDictionary is null)
                {
                    if (originalDictionary is null)
                    {
                        workingDictionary = new Dictionary<string, string>();
                    }
                    else
                    {
                        workingDictionary = new Dictionary<string, string>(originalDictionary);
                    }
                }

                workingDictionary[key] = value;
            }
        }
    }
}
