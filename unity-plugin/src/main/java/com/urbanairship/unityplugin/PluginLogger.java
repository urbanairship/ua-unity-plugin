/* Copyright Airship and Contributors */

package com.urbanairship.unityplugin;

import android.support.annotation.NonNull;
import android.support.annotation.Nullable;
import android.support.annotation.RestrictTo;
import android.util.Log;

import com.urbanairship.LoggingCore;

/**
 * Plugin logger for Airship.
 */
public final class PluginLogger {

    @NonNull
    private static final String TAG = "UALib-Unity";
    private static LoggingCore logger = new LoggingCore(Log.ERROR, TAG);

    /**
     * Private, unused constructor
     */
    private PluginLogger() {}

    /**
     * Sets the log level.
     *
     * @param logLevel The log level.
     * @hide
     */
    @RestrictTo(RestrictTo.Scope.LIBRARY_GROUP)
    public static void setLogLevel(int logLevel) {
        logger.setLogLevel(logLevel);
    }

    /**
     * Send a warning log message.
     *
     * @param message The message you would like logged.
     * @param args The message args.
     * @hide
     */
    @RestrictTo(RestrictTo.Scope.LIBRARY_GROUP)
    public static void warn(@NonNull String message, @Nullable Object... args) {
        logger.log(Log.WARN, null, message, args);
    }

    /**
     * Send a warning log message.
     *
     * @param t An exception to log
     * @param message The message you would like logged.
     * @param args The message args.
     * @hide
     */
    @RestrictTo(RestrictTo.Scope.LIBRARY_GROUP)
    public static void warn(@NonNull Throwable t, @NonNull String message, @Nullable Object... args) {
        logger.log(Log.WARN, t, message, args);
    }

    /**
     * Send a warning log message.
     *
     * @param t An exception to log
     * @hide
     */
    @RestrictTo(RestrictTo.Scope.LIBRARY_GROUP)
    public static void warn(@NonNull Throwable t) {
        logger.log(Log.WARN, t, null, (Object[]) null);
    }

    /**
     * Send a verbose log message.
     *
     * @param message The message you would like logged.
     * @param args The message args.
     * @hide
     */
    @RestrictTo(RestrictTo.Scope.LIBRARY_GROUP)
    public static void verbose(@NonNull String message, @Nullable Object... args) {
        logger.log(Log.VERBOSE, null, message, args);
    }

    /**
     * Send a debug log message.
     *
     * @param message The message you would like logged.
     * @param args The message args.
     * @hide
     */
    @RestrictTo(RestrictTo.Scope.LIBRARY_GROUP)
    public static void debug(@NonNull String message, @Nullable Object... args) {
        logger.log(Log.DEBUG, null, message, args);
    }

    /**
     * Send a debug log message.
     *
     * @param t An exception to log
     * @param message The message you would like logged.
     * @param args The message args.
     * @hide
     */
    @RestrictTo(RestrictTo.Scope.LIBRARY_GROUP)
    public static void debug(@NonNull Throwable t, @NonNull String message, @Nullable Object... args) {
        logger.log(Log.DEBUG, t, message, args);
    }

    /**
     * Send an info log message.
     *
     * @param message The message you would like logged.
     * @param args The message args.
     * @hide
     */
    @RestrictTo(RestrictTo.Scope.LIBRARY_GROUP)
    public static void info(@NonNull String message, @NonNull Object... args) {
        logger.log(Log.INFO, null, message, args);
    }

    /**
     * Send an info log message.
     *
     * @param t An exception to log
     * @param message The message you would like logged.
     * @param args The message args.
     * @hide
     */
    @RestrictTo(RestrictTo.Scope.LIBRARY_GROUP)
    public static void info(@NonNull Throwable t, @NonNull String message, @Nullable Object... args) {
        logger.log(Log.INFO, t, message, args);
    }

    /**
     * Send an error log message.
     *
     * @param message The message you would like logged.
     * @param args The message args.
     * @hide
     */
    @RestrictTo(RestrictTo.Scope.LIBRARY_GROUP)
    public static void error(@NonNull String message, @Nullable Object... args) {
        logger.log(Log.ERROR, null, message, args);
    }

    /**
     * Send an error log message.
     *
     * @param t An exception to log
     * @hide
     */
    @RestrictTo(RestrictTo.Scope.LIBRARY_GROUP)
    public static void error(@NonNull Throwable t) {
        logger.log(Log.ERROR, t, null, (Object[]) null);
    }

    /**
     * Send an error log message.
     *
     * @param t An exception to log
     * @param message The message you would like logged.
     * @param args The message args.
     * @hide
     */
    @RestrictTo(RestrictTo.Scope.LIBRARY_GROUP)
    public static void error(@NonNull Throwable t, @NonNull String message, @Nullable Object... args) {
        logger.log(Log.ERROR, t, message, args);
    }
}