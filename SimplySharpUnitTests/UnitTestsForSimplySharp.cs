using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using SimplySharp.Controllers;
using SimplySharp.Data;
using SimplySharp.Models;
using Microsoft.AspNetCore.Identity;
using SimplySharp.Areas.Identity.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc.ViewFeatures;


namespace SimplySharpUnitTests
{
    [TestClass]
    public class UnitTestsForSimplySharp
    {
        string connectionString = "Server=tcp:lmssimplysharpdbserver.database.windows.net,1433;Initial Catalog=SimplySharp_db;Persist Security Info=False;User ID=SimplySharp;Password=Password$;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        //string connectionString = "Data Source=titan.cs.weber.edu,10433;Initial Catalog=LMSSimplySharp;User ID=LMSSimplySharp;Password=Password$implysharp;";

        //Contexts
        SimplySharpDBContext _db;
        ClassContext _class;
        AssignmentContext _assignment;
        SubmissionContext _submission;
        UserManager<SimplySharpUser> _userManager;

        //Controllers
        ClassesController classesController;
        AssignmentsController assignmentController;
        SubmissionsController submissionsController;
        ClassRegistrationController registrationController;
        PaymentController paymentController;

        IWebHostEnvironment _environment;
        ServiceCollection services;
        Random random;

        [TestInitialize]
        public void TestInitialize()
        {
            services = new ServiceCollection();
            services.AddDbContext<SimplySharpDBContext>(options => options.UseSqlServer(connectionString));
            services.AddDbContext<ClassContext>(options => options.UseSqlServer(connectionString));
            services.AddDbContext<AssignmentContext>(options => options.UseSqlServer(connectionString));
            services.AddDbContext<SubmissionContext>(options => options.UseSqlServer(connectionString));
            var provider = services.BuildServiceProvider();

            _db = provider.GetRequiredService<SimplySharpDBContext>();
            _class = provider.GetRequiredService<ClassContext>();
            _assignment = provider.GetRequiredService<AssignmentContext>();
            _submission = provider.GetRequiredService<SubmissionContext>();

            classesController = new ClassesController(_class, _userManager, _db);
            assignmentController = new AssignmentsController(_assignment, _userManager, _db, _class, _submission);
            submissionsController = new SubmissionsController(_submission, _userManager, _db, _submission, _environment);
            registrationController = new ClassRegistrationController(_class, _userManager, _db);
            paymentController = new PaymentController(_class, _userManager, _db);
            random = new Random();
        }

        [TestMethod]
        public async Task InstructorCanCreateCourse()
        {
            /// Arrange \\\
            //List of all classes in database
            var preTestClassList = await _class.Class.ToListAsync();

            //Select a random index and random instructor
            int randomIndex = random.Next(0, preTestClassList.Count);
            string randomInstructor = preTestClassList[randomIndex].Instructor;

            //Get list of all classes random instructor teaches.
            var preTestClassListForAnInstructor = preTestClassList.FindAll(x => x.Instructor == randomInstructor);
            int preTestCount = preTestClassListForAnInstructor.Count;

            //Make random instructor create a new class
            Class testClass = new Class()
            {
                ClassId = "CS Unit Test",
                ClassName = "Testing Create Course",
                Credits = 4,
                Capacity = 30,
                Location = "Unit Testing Center",
                MeetingDays = "MW",
                StartTime = DateTime.Now,
                EndTime = DateTime.Now,
                Department = "ACTG",
                Instructor = randomInstructor

            };

            /// Act \\\
            await classesController.Create(testClass);

            //After Find out how many courses instructor is teaching and compare to initial count
            var postTestClassList = await _class.Class.ToListAsync();
            var postTestClassListForAnInstructor = postTestClassList.FindAll(x => x.Instructor == randomInstructor);
            int postTestCount = postTestClassListForAnInstructor.Count;

            Debug.WriteLine("Random Instructor Selected: " + randomInstructor);
            Debug.Write("Number classes for instructor before creating test class: ");
            Debug.WriteLine(preTestCount);
            Debug.Write("Number of classes for instructor after creating test class: ");
            Debug.WriteLine(postTestCount);

            //Delete the test class
            await classesController.DeleteConfirmed(testClass.Id);
            var postDeleteClassList = await _class.Class.ToListAsync();
            var postDeleteForInstructor = postDeleteClassList.FindAll(x => x.Instructor == randomInstructor);
            Debug.WriteLine("\nThe test class was deleted.");

            /// Assert \\\
            //Ensure no real classes were deleted
            CollectionAssert.AreEqual(preTestClassListForAnInstructor, postDeleteForInstructor);

            Assert.IsTrue(postTestCount == preTestCount + 1);

        }

        [TestMethod]
        public async Task InstructorCanCreateAssignment()
        {
            /// Arrange \\\
             //List of all classes in database
            var preTestClassList = await _class.Class.ToListAsync();

            //Select a random index and random instructor
            int randomIndex = random.Next(0, preTestClassList.Count);
            string randomInstructor = preTestClassList[randomIndex].Instructor;

            //Get list of all classes random instructor teaches.
            var preTestClassListForAnInstructor = preTestClassList.FindAll(x => x.Instructor == randomInstructor);

            //Select a random index and random class to add assignment to
            randomIndex = random.Next(0, preTestClassListForAnInstructor.Count);
            int randomClassId = preTestClassListForAnInstructor[randomIndex].Id;

            // Get the assignments for the class before the new assignment is created
            var assignmentsPre = await _class.Assignment.ToListAsync();
            var preTestClassAssignments = assignmentsPre.FindAll(x => x.ClassId == randomClassId);
            int N = preTestClassAssignments.Count;
            Debug.WriteLine("Number of assignments for the class before a new assignment is created: " + N.ToString() + "\n");
            foreach (var assignment in preTestClassAssignments) { Debug.WriteLine(assignment.Id.ToString() + "  " + assignment.Title); }

            // Call Assignment Controller action "Create" and pass all relevent info for creating assignment
            Assignment newAssignment = new()
            {
                ClassId = randomClassId,
                Title = $"Unit Test Assignment for class {randomClassId}",
                Description = "This is a unit test assignment 29",
                DueDate = System.DateTime.Now.AddDays(7),
                MaxPoints = 300,
                SubmissionType = "Text Entry"
            };

            /// Act \\\
            await assignmentController.Create(newAssignment, true);

            // Check results of the unit test
            Debug.WriteLine("The new assignment is created: " + newAssignment.Title + "\n");
            var assignmentsPost = await _class.Assignment.ToListAsync();
            var postTestClassAssignments = assignmentsPost.FindAll(x => x.ClassId == randomClassId);
            int N2 = postTestClassAssignments.Count;
            Debug.WriteLine("Number of assignments for the class after the new assignment is created: " + N2.ToString() + "\n");
            foreach (var assignment in postTestClassAssignments) { Debug.WriteLine(assignment.Id.ToString() + "  " + assignment.Title); }

            //Delete the test assignment
            await assignmentController.DeleteConfirmed(newAssignment.Id, true);
            Debug.WriteLine("\nThe test assignment deleted");

            //Update the list of assignments after deletion for compariosn  CollectionAssert.AreEqual
            assignmentsPost = await _class.Assignment.ToListAsync();
            postTestClassAssignments = assignmentsPost.FindAll(x => x.ClassId == randomClassId);

            /// Assert \\\
            //Ensure no real assignments were deleted
            CollectionAssert.AreEqual(preTestClassAssignments, postTestClassAssignments);
            Assert.IsTrue(N2 == N + 1);
        }

        [TestMethod]
        public async Task InstructorCanDropCourse()
        {
            
             
            var classList = await _class.Class.ToListAsync();

            

            //Get a random instructor for the fake class created
            int randomIndex = random.Next(0, classList.Count);
            int randomClass;
            Int32.TryParse(classList[randomIndex].ClassId, out randomClass);
            string randomInstructor = classList[randomIndex].Instructor;
            int regId = classList[randomIndex].Id;

            


            //Make random instructor create a new class
            Class testClass = new Class()
            {
                ClassId = "CS Unit Test",
                ClassName = "Testing Create Course",
                Credits = 4,
                Capacity = 30,
                Location = "Unit Testing Center",
                MeetingDays = "MW",
                StartTime = DateTime.Now,
                EndTime = DateTime.Now,
                Department = "ACTG",
                Instructor = randomInstructor

            };

            //add the class
            await classesController.Create(testClass);


            //calculate total classes of this instructor before deletion
            int beforeDeletionClassCount = 0;
            classList = await _class.Class.ToListAsync();  //get the class list again because we added another class

            foreach (var classroom in classList)
            {
                if (classroom.Instructor == randomInstructor)
                {
                    beforeDeletionClassCount++;
                }
            }

            //write total classes out
            Debug.WriteLine("Total Classes this instructor teaches before deletion: " + beforeDeletionClassCount);

            //find the newly created classID
            var newClass = classList.Find(x => x.ClassId == "CS Unit Test");
            var classID = newClass.Id;
            
            //delete
            await classesController.DeleteConfirmed(classID);

            //recalculate classes the isntructor taught
            int afterDeletionClassCount = 0;
            classList = await _class.Class.ToListAsync(); //get the new classlist again

            foreach (var classroom in classList)
            {
                if (classroom.Instructor == randomInstructor)
                {
                    afterDeletionClassCount++;
                }
            }
            Debug.WriteLine("Total Classes this instructor teaches after deletion: " + afterDeletionClassCount);

            Assert.IsTrue(beforeDeletionClassCount == afterDeletionClassCount + 1);
        }

        [TestMethod]
        public async Task InstructorCanGradeAssignmentBrenton()
        {
            /// Arrange \\\
            var preTestSubmissionList = await _class.Submission.ToListAsync();
            int preTestSubmissionCount = preTestSubmissionList.Count;

            /// Act \\\
            string studentId = "f14cb9d7-81d4-49b0-8572-db2607752e11";
            int assignmentId = 14;
            Submission newTestSubmission = new()
            {
                AssignmentId = assignmentId,
                UserId = studentId,
                Text = "This is a test generated submission. Please disregard.",
                SubmissionDate = DateTime.Now
            };

            await submissionsController.Create(null, newTestSubmission, true, studentId);

            preTestSubmissionList = await _class.Submission.ToListAsync(); //reload the list

            var newSubmissionId = preTestSubmissionList.Find(x => x.AssignmentId == assignmentId).Id; //get the id

            var subCountBefore = preTestSubmissionList.Count();  // get the count for debug and assertion

            Debug.WriteLine("SubmissionCount Before: " +subCountBefore);  //check to make sure the submission wad added

            await submissionsController.Edit(newSubmissionId, newTestSubmission, true); //add a grade (in testing, found that text entries can be graded with blanks)

            Thread.Sleep(2000);

            Debug.WriteLine("Assignment Graded");  //added to debug to make sure that edit was successfull

            await submissionsController.DeleteConfirmed(newSubmissionId); //delete the submission

            preTestSubmissionList = await _class.Submission.ToListAsync(); //reload the list again 

            var subCountAfter = preTestSubmissionList.Count();  // get the count for debug and assertion

            Debug.WriteLine("SubmissionCount Before: " + subCountAfter);  //check to make sure the submission wad added

            Assert.IsTrue(subCountAfter == subCountBefore - 1);
        }

        ///////////////////////
        [TestMethod]
        public async Task StudentCanRegisterForClass()
        {

            /// Arrange \\\
            //List of all classes in database
            var preTestClassList = await _class.Class.ToListAsync();
            var preTestRegistrationList = await _class.ClassRegistration.ToListAsync();

            //Select a random class and random student from a random index 
            int randomIndex = random.Next(0, preTestClassList.Count);
            int randomStuIndex = random.Next(0, preTestRegistrationList.Count);
            int randomClass = Convert.ToInt32(preTestClassList[randomIndex].Id);
            string randomStudent = preTestRegistrationList[randomStuIndex].StudentId;


            //Get list of all classes random student is enrolled in.
            var preTestClassListForAStudent = preTestRegistrationList.FindAll(x => x.StudentId == randomStudent);
            int preTestCount = preTestClassListForAStudent.Count;

            ClassRegistration newRegistration = new()
            {
                ClassId = randomClass,
                StudentId = randomStudent,
                LetterGrade = null

            };


            /// Act \\\ --Add the random class to the random students registration
            await registrationController.Create(randomClass, randomStudent, newRegistration);

            //After Find out how many classes student is registered now and compare to intial count
            var postTestRegistrationList = await _class.ClassRegistration.ToListAsync();
            var postTestClassListForAStudent = postTestRegistrationList.FindAll(x => x.StudentId == randomStudent);
            int postTestCount = postTestClassListForAStudent.Count;

            Debug.WriteLine("Random Student Selected: " + randomStudent);
            Debug.Write("Number of classes student was registered for before the test: ");
            Debug.WriteLine(preTestCount);
            Debug.Write("Number of classes student was registered for after the test: ");
            Debug.WriteLine(postTestCount);

            //Delete the test class
            int regIndex = postTestRegistrationList.FindIndex(x => x.StudentId == randomStudent && x.ClassId == randomClass);
            int regId = postTestRegistrationList[regIndex].Id;
            await registrationController.DeleteConfirmed(regId);
            var postDeleteRegList = await _class.ClassRegistration.ToListAsync();
            var postDeleteForStudent = postDeleteRegList.FindAll(x => x.StudentId == randomStudent);
            Debug.WriteLine("\nThe test registration was deleted.");

            /// Assert \\\
            //Ensure no real classes were deleted
            CollectionAssert.AreEqual(preTestClassListForAStudent, postDeleteForStudent);

            Assert.IsTrue(postTestCount == preTestCount + 1);

        }

        [TestMethod]
        public async Task StudentCanSubmitNewTextAssignment()
        {
            /// Arrange \\\
            var preTestSubmissionList = await _class.Submission.ToListAsync();
            int preTestSubmissionCount = preTestSubmissionList.Count;

            /// Act \\\
            string studentId = "f14cb9d7-81d4-49b0-8572-db2607752e11";
            int assignmentId = 14;
            Submission newTestSubmission = new()
            {
                AssignmentId = assignmentId,
                UserId = studentId,
                Text = "This is a test generated submission. Please disregard.",
                SubmissionDate = DateTime.Now
            };

            Debug.WriteLine("Test submission ready for AssignmentID: " + assignmentId.ToString());
            await submissionsController.Create(null, newTestSubmission, true, studentId);

            int testSubID = newTestSubmission.Id;
            var postTestSubmissionlist = await _class.Submission.ToListAsync();
            int postTestSubmissionCount = postTestSubmissionlist.Count;

            Debug.WriteLine("Test submission successful for SubmissionID : " + testSubID + ".\nSubmission count before test: " + preTestSubmissionCount + "\nSubmission count after test: " + postTestSubmissionCount);

            //delete test submission
            Debug.WriteLine("Now deleting test submission...");
            await submissionsController.DeleteConfirmed(testSubID);
            var postDeleteSubmissionList = await _class.Submission.ToListAsync();

            /// Assert \\\
            CollectionAssert.AreEqual(preTestSubmissionList, postDeleteSubmissionList);
            Assert.IsTrue(preTestSubmissionCount + 1 == postTestSubmissionCount);
        }

        [TestMethod]
        public async Task StudentCanDropClass()
        {

            /// Arrange \\\
            //List of all Registrations
            var preTestRegistrationList = await _class.ClassRegistration.ToListAsync();

            //Select a random registration
            int randomIndex = random.Next(0, preTestRegistrationList.Count);
            int randomClass = Convert.ToInt32(preTestRegistrationList[randomIndex].ClassId);
            string randomStudent = preTestRegistrationList[randomIndex].StudentId;
            int regId = preTestRegistrationList[randomIndex].Id;

            //Get list of all classes random student is enrolled in.
            var preTestClassListForAStudent = preTestRegistrationList.FindAll(x => x.StudentId == randomStudent);
            int preTestCount = preTestClassListForAStudent.Count;


            //Save the class registration information
            ClassRegistration newRegistration = new()
            {
                ClassId = randomClass,
                StudentId = randomStudent,
                LetterGrade = null

            };

            /// Act  --Drop the random registration
            await registrationController.DeleteConfirmed(regId);
            //

            //After Find out how many classes student is registered now and compare to intial count (should be one less)
            var postTestRegistrationList = await _class.ClassRegistration.ToListAsync();
            var postTestClassListForAStudent = postTestRegistrationList.FindAll(x => x.StudentId == randomStudent);
            int postTestCount = postTestClassListForAStudent.Count;

            Debug.WriteLine("Random Student Selected: " + randomStudent);
            Debug.Write("Number of classes student was registered for before the test: ");
            Debug.WriteLine(preTestCount);
            Debug.Write("Number of classes student was registered for after the test: ");
            Debug.WriteLine(postTestCount);

            //Re-register student for class
            await registrationController.Create(randomClass, randomStudent, newRegistration);

            //int regIndex = postTestRegistrationList.FindIndex(x => x.StudentId == randomStudent && x.ClassId == randomClass);
            //int regId = postTestRegistrationList[regIndex].Id;

            var postAddRegList = await _class.ClassRegistration.ToListAsync();
            var postAddForStudent = postAddRegList.FindAll(x => x.StudentId == randomStudent);
            Debug.WriteLine("\nThe dropped class was added back to the students registration.");

            /// Assert \\\           
            Assert.IsTrue(postTestCount == preTestCount - 1);
        }


        [TestMethod]
        public async Task StudentCanPayTuition()
        {
            /// Arrange \\\

            //List of all classes in database
            var preTestTotalPayments = await _class.Payment.ToListAsync();

            //Get list of all payments for the test student before the new payment is created
            string studentId = "f14cb9d7-81d4-49b0-8572-db2607752e11";
            var preTestTotalPaymentsForStudent = preTestTotalPayments.FindAll(x => x.UserId == studentId);
            int N = preTestTotalPaymentsForStudent.Count;
            Debug.WriteLine("Number of payments for the student before a new payment is created: " + N.ToString() + "\n");
            foreach (var payment in preTestTotalPaymentsForStudent) { Debug.WriteLine(payment.PaymentAmount.ToString("$#,###.00") + " on " + payment.PaymentDate.ToString("yyyy-MM-dd HH:mm")); }

            /// Act \\\
            // Call Payment Controller action "Submit" and pass all relevent info for creating a payment
            await paymentController.Submit("4242424242424242", "12", "2023", "123", "25.25", "Unit Test Payment", true, studentId);

            // Check results of the unit test
            var postTestTotalPayments = await _class.Payment.ToListAsync();
            var postTestTotalPaymentsForStudent = postTestTotalPayments.FindAll(x => x.UserId == studentId);
            int N2 = postTestTotalPaymentsForStudent.Count;
            Debug.WriteLine("Number of payments for the student after the new payment is created: " + N2.ToString() + "\n");
            foreach (var payment in postTestTotalPaymentsForStudent) { Debug.WriteLine(payment.PaymentAmount.ToString("$#,###.00") + " on " + payment.PaymentDate.ToString("yyyy-MM-dd HH:mm")); }

            //Delete the test payment
            //var addedPayment = await _class.Payment.FindAsync(paymentId);
            var addedPayment = postTestTotalPaymentsForStudent[N2 - 1];
            _class.Payment.Remove(addedPayment);
            await _class.SaveChangesAsync();
            Debug.WriteLine("\nThe test payment deleted");

            //Update the list of assignments after deletion for compariosn  CollectionAssert.AreEqual
            postTestTotalPayments = await _class.Payment.ToListAsync();
            postTestTotalPaymentsForStudent = postTestTotalPayments.FindAll(x => x.UserId == studentId);

            /// Assert \\\
            //Ensure no real assignments were deleted
            CollectionAssert.AreEqual(preTestTotalPayments, postTestTotalPayments);
            Assert.IsTrue(N2 == N + 1);
        }

        [TestMethod]
        public async Task InstructorCanGradeAssignment()
        {
            /// Arrange \\\
            //Add a test assignment.
            Assignment testAssignment = new()
            {
                ClassId = 20,
                Title = $"Unit Test Assignment for class {20}",
                Description = "This is a unit test assignment xx",
                DueDate = System.DateTime.Now.AddDays(7),
                MaxPoints = 300,
                SubmissionType = "Text Entry"
            };
            await assignmentController.Create(testAssignment, true);

            //Get list of all submissions and then add a new submission to database.
            var preTestSubmissions = await _submission.Submission.ToListAsync();
            string studentId = "e9332579-2879-4ae3-8a15-bab4fa07e176";
            int assignmentId = testAssignment.Id;
            Submission testSubmission = new()
            {
                AssignmentId = assignmentId,
                UserId = studentId,
                Text = "This is a test generated submission. Please disregard.",
                SubmissionDate = DateTime.Now
            };
            await submissionsController.Create(null, testSubmission, true, studentId);

            //Keep track of the score before grading the assignment.
            int? preTestScore = testSubmission.Score;

            /// Act \\\
            //Grade assignment.
            testSubmission.Score = 1;
            //Submit the grade.
            await submissionsController.Edit(testSubmission.Id, testSubmission, true);

            //Get a list of submissions after grading assignment.
            var postTestSubmissions = await _submission.Submission.ToListAsync();
            //Set test submission to the newly graded version of the submission.
            testSubmission = postTestSubmissions.Find(x => x.Id == testSubmission.Id);
            //Save the score for assertion.
            int? postTestScore = testSubmission.Score;

            //Delete test assignment and submission.
            await assignmentController.DeleteConfirmed(testAssignment.Id, true);
            await submissionsController.DeleteConfirmed(testSubmission.Id);

            Debug.WriteLine("Score before grading: null");
            Debug.Write("Score after grading: ");
            Debug.WriteLine(postTestScore);

            /// Assert \\\
            //Ensure the score was posted.
            Assert.IsNotNull(postTestScore);
            Assert.AreNotEqual(preTestScore, postTestScore);
        }

        [TestMethod]
        public async Task StudentCanResubmitTextAssignment()
        {
            /// Arrange \\\
            // List of all submissions in database
            var preTestSubmissionList = await _class.Submission.ToListAsync();

            /// Act \\\
            // Select a random already submitted text assignment, and identify the submitting student and assignment
            var testSubmission = preTestSubmissionList.Find(x => x.Text != null);
            int testAssignmentID = testSubmission.AssignmentId;
            string testStudentID = testSubmission.UserId;

            // Save the old submission text, then resubmit to the same assignment with test text
            string originalText = testSubmission.Text;
            string testText = "Unit Test - Resubmission Text";

            Debug.WriteLine("Original submission targeted; current text is: " + originalText);
            testSubmission.Text = testText;
            Debug.WriteLine("Resubmission text altered; ready for resubmit");

            await submissionsController.Edit(testSubmission.Id, testSubmission, true);

            // Check that the resubmission happened
            Debug.WriteLine("Resubmit successful. Refinding submission in DB and comparing text to original.");

            var postChangeSubmissionList = await _class.Submission.ToListAsync();
            var postChangeSubmission = postChangeSubmissionList.Find(x => x.Id == testSubmission.Id);
            Debug.WriteLine("Resubmit found; text is: " + postChangeSubmission.Text);
            if (postChangeSubmission.Text == testText)
            {
                Debug.WriteLine("Resubmit successful; targeted submission text is equal to test text string.");
            }
            else
            {
                Debug.WriteLine("Resubmit failed; targeted submission text IS NOT EQUAL to test text string.");
            }

            Debug.WriteLine("Reverting test submission to original text");
            // Revert submission to the old text
            postChangeSubmission.Text = originalText;
            await submissionsController.Edit(postChangeSubmission.Id, postChangeSubmission, true);

            /// Assert \\\
            //Check that test submission was reverted to the original
            var postTestSubmissionList = await _class.Submission.ToListAsync();
            var postTestSubmission = postTestSubmissionList.Find(x => x.Id == testSubmission.Id);

            Assert.IsTrue(testSubmission == postTestSubmission);
            Assert.IsTrue(originalText == postTestSubmission.Text);
        }
    }
}

