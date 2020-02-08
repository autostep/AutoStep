using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using FluentAssertions;
using Xunit;

namespace AutoStep.Tests.Language.Interaction
{
    /// <summary>
    /// Trait graph behaviour
    ///     - Add all traits to a sorted set of traits, ordered by number of referenced traits.    
    ///     - ComputeDependences - Starting at the trait with the most dependencies:
    ///         - Look at the traits it depends on.
    ///         - Find the next trait down the list where all the traits of that parent are in the dependency list of the current item.
    ///         - Mark those dependencies as processed in the child.
    ///         - Recurse for that found parent.
    ///         - When recursion has exited, we add the 
    ///         
    ///     - SearchDependencies.
    ///         - Takes a set of traits
    /// 
    /// </summary>
    public class TraitGraph
    {
        public LinkedList<TraitNode> AllTraits { get; } = new LinkedList<TraitNode>();

        public Dictionary<TraitRef, Trait> TraitLookup { get; set; } = new Dictionary<TraitRef, Trait>();

        public void AddOrExtendTrait(Trait newTrait)
        {
            var newRef = newTrait.GetRef();

            if(TraitLookup.TryGetValue(newRef, out var trait))
            {
                // This is just an update to an existing trait.
                // TODO: Migrate all the functions/properties of this duplicate trait
                //       over to the new one.
                trait.ExtendWith(newTrait);

                return;
            }

            // Add a new trait to the sorted set of traits.
            TraitLookup.Add(newRef, newTrait);

            var newNode = new TraitNode 
            { 
                Ref = newRef,
                Trait = newTrait
            };

            //// Construct a set of known refs for the node.
            //newNode.KnownRefs = newRef.ReferencedTraits.Select(t => new TraitRef(t)).ToArray();

            // Find the insertion point.
            var existingNode = AllTraits.First;

            if (existingNode is null)
            {
                AllTraits.AddFirst(newNode);
            }
            else 
            {
                // Go through the set until this new node is less complex than the current node in
                // the list.
                while (newNode.NumberOfReferencedTraits < existingNode.Value.NumberOfReferencedTraits)
                {
                    var nextNode = existingNode.Next;

                    if (nextNode is null)
                    {
                        break;
                    }
                    else
                    {
                        existingNode = existingNode.Next;
                    }
                }

                // Add before the existing node.
                AllTraits.AddBefore(existingNode, newNode);
            }
        }

        public FlattenedTraitSet MatchTraits(string[] namedTraits)
        {
            // Fast path efficiency; all traits must exist at root,
            // so validate that they do (in the TraitLookup) and bail early if not.
            // TODO.
            var applicableTraitSet = new FlattenedTraitSet();

            // Find the current item in the list.
            var currentItem = AllTraits.First;

            // Define a 'processed' array of traits.
            var traitMatchingSet = new TraitNameMatchingSet(namedTraits);
            var consumedTraitMask = 0ul;

            VisitNode(currentItem, traitMatchingSet, ref consumedTraitMask, applicableTraitSet);

            return applicableTraitSet;
        }

        private void VisitNode(LinkedListNode<TraitNode> currentNode, TraitNameMatchingSet matchingSet, ref ulong traitConsumedMask, FlattenedTraitSet traitResultSet)
        {
            while (matchingSet.AnyLeft(traitConsumedMask) && currentNode is object)
            {
                // Find the next node in the list that is entirely consumed.
                var traitNode = currentNode.Value;

                // If the set of references for the current set item is fully contained by the test ref...
                if (traitNode.ConsumeIfEntirelyContainedIn(matchingSet, ref traitConsumedMask))
                {
                    // Everything in this item is applicable to the set of traits.
                    // Consume it.

                    if (traitNode.NumberOfReferencedTraits > 0)
                    {
                        if (traitNode.HaveDeterminedParents)
                        {
                            // We know the parents of this node already, so just append all parents to 
                            // the list.
                            traitResultSet.Merge(traitNode.ApplicableTraitSet);

                        }
                        else
                        {
                            // We have not visited this tree item before, visit it.
                            // Create a new flattened trait set.
                            traitNode.ApplicableTraitSet = new FlattenedTraitSet();

                            // The trait mask for pursuing this path is only those things already consumed by this 
                            // path.
                            var nextStageMask = ~traitConsumedMask;

                            VisitNode(currentNode.Next, matchingSet, ref nextStageMask, traitNode.ApplicableTraitSet);

                            traitConsumedMask |= ~nextStageMask;

                            traitResultSet.Merge(traitNode.ApplicableTraitSet);
                        }
                    }

                    // Add self.
                    traitResultSet.Add(traitNode.Trait);
                }
                
                // Not a match, try the next one in the set.
                currentNode = currentNode.Next;                
            }
        }
    }

    public struct ConsumedTrait
    {
        public bool IsConsumed { get; set; }

        public string Name { get; set; }
    }

    public class FlattenedTraitSet
    {
        /// <summary>
        /// The last trait is the most specific one.
        /// </summary>
        public List<Trait> OrderedTraits { get; } = new List<Trait>();

        public void Merge(FlattenedTraitSet applicableTraitSet)
        {
            OrderedTraits.AddRange(applicableTraitSet.OrderedTraits);
        }

        public void Add(Trait trait)
        {
            OrderedTraits.Add(trait);
        }
    }
    
    [DebuggerDisplay("{DebuggerToString()}")]
    public struct TraitNode
    {
        public TraitRef Ref { get; set; }

        public Trait Trait { get; set; }

        public bool HaveDeterminedParents => ApplicableTraitSet is object;

        public int NumberOfReferencedTraits => Ref.NumberOfReferencedTraits;
        
        public string DebuggerToString()
        {
            return Ref.DebuggerToString();
        }

        public FlattenedTraitSet ApplicableTraitSet { get; set; }

        public bool ConsumeIfEntirelyContainedIn(TraitNameMatchingSet traits, ref ulong consumedMask)
        {
            if(Ref.TopLevelName is object)
            {
                // If this method returns true, the set will have consumed that 
                return traits.ConsumeIfContains(Ref.TopLevelName, ref consumedMask);
            }

            return traits.ConsumeIfContains(Ref.ReferencedTraits, ref consumedMask);
        }
    }

    public class TraitOrderComparer : IComparer<TraitNode>
    {
        public int Compare(TraitNode x, TraitNode y)
        {
            return x.Ref.NumberOfReferencedTraits.CompareTo(y.Ref.NumberOfReferencedTraits);
        }
    }

    [DebuggerDisplay("{DebuggerToString()}")]
    public struct TraitRef : IEquatable<TraitRef>
    {
        public TraitRef(string singleComponent)
        {
            TopLevelName = singleComponent;
            ReferencedTraits = Array.Empty<string>();
        }

        public TraitRef(string[] sortedComponents)
        {
            if (sortedComponents.Length > 1)
            {
                ReferencedTraits = sortedComponents;
                TopLevelName = null;
            }
            else
            {
                TopLevelName = sortedComponents[0];
                ReferencedTraits = Array.Empty<string>();
            }
        }

        public int NumberOfReferencedTraits => ReferencedTraits.Length;

        public string TopLevelName { get; set; }

        public string[] ReferencedTraits { get; set; }

        public string DebuggerToString()
        {
            return TopLevelName ?? string.Join(" + ", ReferencedTraits);
        }

        public override bool Equals(object obj)
        {
            return obj is TraitRef @ref && Equals(@ref);
        }

        public bool Equals([AllowNull] TraitRef other)
        {
            return TopLevelName == other.TopLevelName &&
                   EqualityComparer<string[]>.Default.Equals(ReferencedTraits, other.ReferencedTraits);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TopLevelName, ReferencedTraits);
        }

        public static bool operator ==(TraitRef left, TraitRef right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TraitRef left, TraitRef right)
        {
            return !(left == right);
        }
    }

    [DebuggerDisplay("{DebuggerToString()}")]
    public class Trait
    {
        public Trait(params string[] nameComponents)
        {
            if(nameComponents.Length == 0)
            {
                throw new ArgumentException();
            }

            // Sort the components so each identical trait (i.e. same combination of parent traits) has exactly the same
            // value.
            Array.Sort(nameComponents);
            NameParts = nameComponents;            
        }

        public TraitRef GetRef()
        {
            return new TraitRef(NameParts);
        }

        internal void ExtendWith(Trait newTrait)
        {
            throw new NotImplementedException();
        }

        public string DebuggerToString()
        {
            return string.Join(" + ", NameParts);
        }

        public int NumberOfReferencedTraits => NameParts.Length;

        public string[] NameParts { get; set; }
    }

    public class TraitGraphTests
    {
        [Fact]
        public void SimpleGraphCreate()
        {
            var traitGraph = new TraitGraph();

            var traitA = new Trait("A");
            var traitB = new Trait("B");
            var traitC = new Trait("C");
            var traitD = new Trait("D");

            var traitAD = new Trait("A", "D");
            var traitBD = new Trait("B", "D");

            traitGraph.AddOrExtendTrait(traitA);
            traitGraph.AddOrExtendTrait(traitB);

            traitGraph.AddOrExtendTrait(traitAD);
            traitGraph.AddOrExtendTrait(traitC);
            traitGraph.AddOrExtendTrait(traitD);
            traitGraph.AddOrExtendTrait(traitBD);

            // Find all traits.
            var result = traitGraph.MatchTraits(new string[] { "A", "D", "C" });

            result.OrderedTraits.Should().BeEquivalentTo(traitD, traitA, traitAD, traitC);
        }

        [Fact]
        public void ComplexGraphCreate()
        {
            var traitGraph = new TraitGraph();

            var traitA = new Trait("A");
            var traitB = new Trait("B");
            var traitC = new Trait("C");
            var traitD = new Trait("D");
            var traitE = new Trait("E");
            var traitF = new Trait("F");
            var traitG = new Trait("G");
            
            var traitAB = new Trait("A", "B");
            var traitBD = new Trait("B", "D");
            var traitCD = new Trait("C", "D");
            var traitCE = new Trait("C", "E");
            var traitABC = new Trait("A", "B", "C");
            var traitCDE = new Trait("C", "D", "E");
            var traitABCD = new Trait("A", "B", "C", "D");

            traitGraph.AddOrExtendTrait(traitA);
            traitGraph.AddOrExtendTrait(traitB);
            traitGraph.AddOrExtendTrait(traitC);
            traitGraph.AddOrExtendTrait(traitD);
            traitGraph.AddOrExtendTrait(traitE);
            traitGraph.AddOrExtendTrait(traitF);
            traitGraph.AddOrExtendTrait(traitG);
            traitGraph.AddOrExtendTrait(traitAB);
            traitGraph.AddOrExtendTrait(traitBD);
            traitGraph.AddOrExtendTrait(traitCD);
            traitGraph.AddOrExtendTrait(traitCE);
            traitGraph.AddOrExtendTrait(traitABC);
            traitGraph.AddOrExtendTrait(traitCDE);
            traitGraph.AddOrExtendTrait(traitABCD);

            // Find all traits.
            var result = traitGraph.MatchTraits(new string[] { "A", "B", "C", "E" });

            result.OrderedTraits.Should().BeEquivalentTo(traitE, traitC, traitB, traitA,
                                                         traitCE, traitAB, 
                                                         traitABC, 
                                                         traitABCD);
        }
    }

    public class TraitNameMatchingSetTests
    {
        [Fact]
        public void CheckConsumesSingleName()
        {
            var nameMatcher = new TraitNameMatchingSet(new[] { "A", "B" });
            var currentMask = 0ul;

            nameMatcher.AnyLeft(currentMask).Should().BeTrue();

            nameMatcher.ConsumeIfContains("A", ref currentMask).Should().BeTrue();

            currentMask.Should().Be(0b01);

            nameMatcher.AnyLeft(currentMask).Should().BeTrue();

            nameMatcher.ConsumeIfContains("B", ref currentMask).Should().BeTrue();

            currentMask.Should().Be(0b11);

            nameMatcher.AnyLeft(currentMask).Should().BeFalse();
        }

        [Fact]
        public void CheckConsumesSet()
        {
            var nameMatcher = new TraitNameMatchingSet(new[] { "A", "B", "C", "D", "E" });
            var currentMask = 0ul;

            nameMatcher.AnyLeft(currentMask).Should().BeTrue();

            nameMatcher.ConsumeIfContains(new[] { "A", "B", "E" }, ref currentMask).Should().BeTrue();

            currentMask.Should().Be(0b10011);

            nameMatcher.AnyLeft(currentMask).Should().BeTrue();

            // Check for non-overlap.
            nameMatcher.ConsumeIfContains(new[] { "D", "E", "F" }, ref currentMask).Should().BeFalse();

            // No change in the mask.
            currentMask.Should().Be(0b10011);

            // Already consumed, should not consume.
            nameMatcher.ConsumeIfContains(new[] { "A", "B", "E" }, ref currentMask).Should().BeFalse();

            // No change in the mask.
            currentMask.Should().Be(0b10011);

            // Consume the remainder.
            nameMatcher.ConsumeIfContains(new[] { "C", "D" }, ref currentMask).Should().BeTrue();

            currentMask.Should().Be(0b11111);

            nameMatcher.AnyLeft(currentMask).Should().BeFalse();
        }
    }
}
