-- Add the runtime libraries to the package path
package.path = package.path .. ';./runtime/lib/?.lua'

require 'Entity'
require 'GameMode'
require 'middleclass'