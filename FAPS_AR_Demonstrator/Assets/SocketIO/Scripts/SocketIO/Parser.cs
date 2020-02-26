#region License
/*
 * Parser.cs
 *
 * The MIT License
 *
 * Copyright (c) 2014 Fabio Panettieri
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion
using UnityEngine;

namespace SocketIO
{
	public class Parser
	{
		public SocketIOEvent Parse(JSONObject json)
		{
            //Debug.Log("[SocketIO HDLE MSG PARSER] EVENT START");
            if (json.Count < 1 || json.Count > 2) {
				throw new SocketIOException("Invalid number of parameters received: " + json.Count);
			}
            //Debug.Log("[SocketIO HDLE MSG PARSER] Valid number of parameters");
            if (json[0].type != JSONObject.Type.STRING) {
				throw new SocketIOException("Invalid parameter type. " + json[0].type + " received while expecting " + JSONObject.Type.STRING);
			}
            // Debug.Log("[SocketIO HDLE MSG PARSER] Valid parameter type");
            if (json.Count == 1) {
                // Debug.Log("[SocketIO HDLE MSG PARSER] Return inly 1 Count");
                return new SocketIOEvent(json[0].str);
			}

            //if (json[1].type != JSONObject.Type.OBJECT) {
			//	throw new SocketIOException("Invalid argument type. " + json[1].type + " received while expecting " + JSONObject.Type.OBJECT);
			//}
            return new SocketIOEvent(json[0].str, json[1]);
		}
	}
}
