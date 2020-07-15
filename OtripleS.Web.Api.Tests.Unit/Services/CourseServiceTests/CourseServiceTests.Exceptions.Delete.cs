﻿// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE AS LONG AS SOFTWARE FUNDS ARE DONATED TO THE POOR
// ---------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Moq;
using OtripleS.Web.Api.Models.Courses;
using OtripleS.Web.Api.Models.Courses.Exceptions;
using Xunit;

namespace OtripleS.Web.Api.Tests.Unit.Services.CourseServiceTests
{
    public partial class CourseServiceTests
    {
        [Fact]
        public async Task ShouldThrowDependencyExceptionOnDeleteWhenSqlExceptionOccursAndLogItAsync()
        {
            // given
            Guid randomCourseId = Guid.NewGuid();
            Guid inputCourseId = randomCourseId;
            SqlException sqlException = GetSqlException();

            var expectedCourseDependencyException =
                new CourseDependencyException(sqlException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectCourseByIdAsync(inputCourseId))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<Course> deleteCourseTask =
                this.courseService.DeleteCourseAsync(inputCourseId);

            // then
            await Assert.ThrowsAsync<CourseDependencyException>(() =>
                deleteCourseTask.AsTask());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCritical(It.Is(SameExceptionAs(expectedCourseDependencyException))),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectCourseByIdAsync(inputCourseId),
                    Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}