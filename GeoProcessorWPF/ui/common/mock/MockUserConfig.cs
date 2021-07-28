#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'GeoProcessorWPF' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;

namespace J4JSoftware.GeoProcessor
{
    public class MockUserConfig : IUserConfig
    {
        public MockUserConfig()
        {
            APIKeys = new Dictionary<ProcessorType, APIKey>();

            foreach( var procType in Enum.GetValues<ProcessorType>() )
            {
                APIKeys.Add( procType, new APIKey { Value = "some API key" } );
            }
        }

        public Dictionary<ProcessorType, APIKey> APIKeys { get; set; }
        
        public UserConfig Copy()
        {
            throw new NotImplementedException();
        }

        public void RestoreFrom( UserConfig src )
        {
            throw new NotImplementedException();
        }

        public string GetAPIKey( ProcessorType procType )
        {
            throw new NotImplementedException();
        }
    }
}