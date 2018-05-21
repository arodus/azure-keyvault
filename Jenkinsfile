pipeline {
    agent { label 'linux' }
    options {
        buildDiscarder(logRotator(numToKeepStr:'10'))
        timeout(time: 15, unit: 'MINUTES')
    }
    stages {
        stage('Compile') {
            steps {
                sh '/bin/bash ./build.sh Compile'
            }
        }
        stage('Pack') {
            steps {
                sh '/bin/bash ./build.sh Pack -Skip -NoInit'
            }
			post {
				success {
					archiveArtifacts 'output/*'
				}
			}
        }
        
    }
}
