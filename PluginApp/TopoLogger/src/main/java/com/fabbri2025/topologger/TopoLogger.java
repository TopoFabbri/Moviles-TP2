package com.fabbri2025.topologger;

import android.util.Log;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;

public class TopoLogger {

    public static final String LOG_TAG = "TopoLogger";

    public enum LogLevel {
        ERROR,
        WARNING,
        INFO,
        DEBUG,
        VERBOSE
    }

    public static void SendLog(String log, LogLevel level)
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
