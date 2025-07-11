/* Copyright Airship and Contributors */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UrbanAirship {

    /// <summary>
    /// Airship InApp Experiences.
    /// </summary>
    public class AirshipInApp {

        private IAirshipPlugin plugin;

        internal AirshipInApp (IAirshipPlugin plugin) {
            this.plugin = plugin;
        }

        /// <summary>
        /// Pauses/resumes messages.
        /// </summary>
        /// <param name="paused"><c>true</c> to pause, <c>false</c> to resume.</param>
        public void SetPaused (bool paused) {
            this.plugin.Call ("setPaused", paused);
        }

        /// <summary>
        /// Checks if messages are paused.
        /// </summary>
        /// <returns><c>true</c> if paused, otherwise <c>false</c></returns>
        public bool IsPaused () {
            return this.plugin.Call<bool> ("isPaused");
        }

        /// <summary>
        /// Sets the display interval for messages.
        /// </summary>
        /// <param name="displayInterval">The display interval.</param>
        public void SetDisplayInterval (TimeSpan displayInterval) {
            this.plugin.Call ("setDisplayInterval", displayInterval);
        }

        /// <summary>
        /// Gets the messages display interval.
        /// </summary>
        /// <returns>The display interval.</returns>
        public TimeSpan GetDisplayInterval () {
            return this.plugin.Call<TimeSpan> ("getDisplayInterval");
        }
    }
}