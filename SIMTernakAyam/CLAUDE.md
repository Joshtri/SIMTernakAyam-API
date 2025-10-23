https://localhost:7195/api/dashboard/petugas


{
    "success": true,
    "message": "Berhasil mengambil dashboard petugas.",
    "data": {
        "myKandangs": [
            {
                "id": "fd082b84-41c8-45d4-8480-ddb918f74cca",
                "name": "Kandang 2",
                "currentAyams": 0,
                "capacity": 1500,
                "mortalityToday": 0,
                "mortalityThisWeek": 0,
                "lastFeedTime": "0001-01-01T00:00:00",
                "lastVaccinationTime": "0001-01-01T00:00:00",
                "healthStatus": "Empty"
            }
        ],
        "dailyTasks": {
            "pendingFeedings": 3,
            "pendingVaccinations": 0,
            "pendingCleanings": 1,
            "completedTasks": 0,
            "totalTasks": 4,
            "completionRate": 0
        },
        "stockAlerts": {
            "lowStockPakan": [],
            "lowStockVaksin": [
                {
                    "id": "350bf96b-3929-4830-8ab9-a47e5829890f",
                    "name": "Sinovac1 2",
                    "currentStock": 2,
                    "minimumStock": 10,
                    "status": "Critical"
                }
            ],
            "criticalStockCount": 1,
            "warningStockCount": 0
        },
        "myPerformance": {
            "efficiencyScore": 50,
            "tasksCompletedThisWeek": 0,
            "tasksCompletedThisMonth": 0,
            "averageMortalityRate": 0,
            "kandangsManaged": 1,
            "performanceLevel": "Needs Improvement"
        },
        "upcomingActivities": {
            "todayActivities": [
                {
                    "activityType": "Feeding",
                    "kandangName": "Kandang 2",
                    "scheduledTime": "2025-10-22T08:00:00Z",
                    "priority": "High",
                    "isOverdue": true
                },
                {
                    "activityType": "Cleaning",
                    "kandangName": "Kandang 2",
                    "scheduledTime": "2025-10-22T14:00:00Z",
                    "priority": "Medium",
                    "isOverdue": true
                }
            ],
            "tomorrowActivities": [],
            "thisWeekActivities": []
        }
    },
    "errors": null,
    "statusCode": 200,
    "timestamp": "2025-10-23T03:33:35.7183396+07:00"
}