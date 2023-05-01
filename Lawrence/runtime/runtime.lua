-- Add the runtime libraries to the package path
package.path = package.path .. ';./runtime/lib/?.lua'

require 'middleclass'
require 'Entity'
require 'Universe'
require 'Player'
require 'Label'
require 'Moby'