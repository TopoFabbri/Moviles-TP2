package com.fabbri2025.topologger;

import android.util.Log;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;

import androidx.annotation.Keep;

@Keep
public class TopoLogger {

    public static final String LOG_TAG = "TopoLogger";

    public enum LogLevel {
        ERROR,
        WARNING,
        INFO,
        DEBUG,
        VERBOSE
    }

    @Keep
    private static void SendLogInternal(String log, LogLevel level)
    {
        switch (level) {
            case ERROR:
                Log.e(LOG_TAG, log);
                break;
            case WARNING:
                Log.w(LOG_TAG, log);
                break;
            case INFO:
                Log.i(LOG_TAG, log);
                break;
            case DEBUG:
                Log.d(LOG_TAG, log);
                break;
            case VERBOSE:
                Log.v(LOG_TAG, log);
                break;
            default:
                break;
        }
    }

    public static void SendLog(String log, int level) {
        LogLevel logLevel;
        switch (level) {
            case 0:
                logLevel = LogLevel.ERROR;
                break;
            case 1:
                logLevel = LogLevel.WARNING;
                break;
            case 2:
                logLevel = LogLevel.INFO;
                break;
            case 3:
                logLevel = LogLevel.DEBUG;
                break;
            case 4:
                logLevel = LogLevel.VERBOSE;
                break;
            default:
                logLevel = LogLevel.INFO; // Default to INFO
                break;
        }
        SendLogInternal(log, logLevel);
    }

    public static void SendLog(String log, String level) {
        LogLevel logLevel;
        if (level.equalsIgnoreCase("error")) {
            logLevel = LogLevel.ERROR;
        } else if (level.equalsIgnoreCase("warning")) {
            logLevel = LogLevel.WARNING;
        } else if (level.equalsIgnoreCase("info")) {
            logLevel = LogLevel.INFO;
        } else if (level.equalsIgnoreCase("debug")) {
            logLevel = LogLevel.DEBUG;
        } else if (level.equalsIgnoreCase("verbose")) {
            logLevel = LogLevel.VERBOSE;
        } else {
            logLevel = LogLevel.INFO; // Default to INFO
        }
        SendLogInternal(log, logLevel);
    }

    @Keep
    public static String GetLogs()
    {
        try {
            Process process = Runtime.getRuntime().exec("logcat -d " + LOG_TAG + ":V *:S");
            BufferedReader bufferedReader = new BufferedReader(
                    new InputStreamReader(process.getInputStream()));

            StringBuilder log = new StringBuilder();
            String line;
            while ((line = bufferedReader.readLine()) != null) {
                log.append(line);
                log.append("\n");
            }
            return log.toString();
        } catch (IOException e) {
            Log.e(LOG_TAG, "Error reading logs", e);
            return null;
        }
    }
}
