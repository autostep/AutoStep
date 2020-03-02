# Interaction Language

- Function: some implemented action or assert.
- Trait: A set of grouped behaviour for an element. E.g. clickable, displayable, etc.
- Component: A control or element on the page that has a defined name.

How do you define how controls/components interact with a page.

Components can have traits and steps.

## Functions
Functions can be written in C#.
They can be global, or attached to traits, or components.
Return value of 1 function can always be passed in to the next one.

Function execution is like a map->reduce pipeline.
First function always receives the top-level document element
Functions can output the same value as the input
Functions can filter or change the input value, which becomes the value passed into the next method


## Traits
Traits can be combined.
Traits can have steps.
Trait steps have a special $component$ value that is expanded into a new step
for each component, with the appropriate name.
Traits can define functions, which can consume built-in functions.

Traits can imply default 'select' behaviour.

singular trait automatically implies locate(), 
named trait automatically implies locateNamed()

## Components
A component is an actual field type.


## Steps
Traits and Components can define Steps.
The presence of a table for a step will be defined by the functions that are 
called by the Step.

A step is a chain of functions.
  
